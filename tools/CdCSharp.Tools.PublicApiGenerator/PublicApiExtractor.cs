using Microsoft.CodeAnalysis;

namespace GeneratePublicApi;

/// <summary>
/// Extrae todos los símbolos públicos/protected de la compilación de un proyecto,
/// filtrando ÚNICAMENTE a los declarados en los ficheros propios del proyecto
/// (no en ensamblados referenciados).
/// </summary>
internal static class PublicApiExtractor
{
    /// <summary>
    /// Devuelve las líneas que deben ir en PublicAPI.Unshipped.txt, ordenadas
    /// y con la cabecera <c>#nullable enable</c> si corresponde.
    /// </summary>
    public static List<string> Extract(
        Compilation compilation,
        HashSet<string> ownFilePaths,
        bool nullableEnable = true)
    {
        // Propagar el flag nullable al formateador antes de extraer símbolos.
        // MSBuildWorkspace puede dejar NullableAnnotation.None en tipos que en
        // realidad son no-nullable; SetNullableEnable permite que TypeStr() añada
        // manualmente el '!' en esos casos.
        SymbolFormatter.SetNullableEnable(nullableEnable);

        List<string> lines = new();

        // Recorremos el árbol de símbolos del ensamblado propio
        List<string> ownSymbols = new();
        CollectFromNamespace(compilation.GlobalNamespace, compilation.Assembly, ownSymbols);

        ownSymbols.Sort(StringComparer.Ordinal);

        if (nullableEnable)
            lines.Add("#nullable enable");

        lines.AddRange(ownSymbols);
        return lines;
    }

    // ── Traversal recursivo ─────────────────────────────────────────────────

    private static void CollectFromNamespace(
        INamespaceSymbol ns,
        IAssemblySymbol ownAssembly,
        List<string> result)
    {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers())
            CollectFromType(type, ownAssembly, result);

        foreach (INamespaceSymbol childNs in ns.GetNamespaceMembers())
            CollectFromNamespace(childNs, ownAssembly, result);
    }

    private static void CollectFromType(
        INamedTypeSymbol type,
        IAssemblySymbol ownAssembly,
        List<string> result)
    {
        // Solo tipos accesibles públicamente
        if (!IsPubliclyVisible(type)) return;

        // Filtrar a símbolos del propio ensamblado (cubre source generators).
        if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, ownAssembly)) return;

        // Añadir el tipo en sí
        result.Add(SymbolFormatter.FormatType(type));

        // Añadir miembros del tipo
        foreach (ISymbol member in type.GetMembers())
        {
            if (!IsPubliclyVisible(member)) continue;

            // Filtrar miembros implícitos, con excepción del constructor sin
            // parámetros de classes y structs.
            //
            // En C# toda class/struct pública que no declara ningún constructor
            // explícito tiene un constructor público sin parámetros generado por
            // el compilador (IsImplicitlyDeclared=true). El analizador RS0016 SÍ
            // lo incluye en PublicAPI.txt porque forma parte del contrato de la API.
            //
            // Además, los tipos `record` y `record struct` tienen miembros
            // sintetizados que también deben aparecer en el fichero:
            //   Deconstruct, Equals(T), Equals(object), GetHashCode, ToString,
            //   operator ==, operator !=
            //
            // Cubre, entre otros:
            //   ✓ `public sealed class BorderCssValues { ... }`   → ctor() implícito
            //   ✓ `public class BUITransitionsBuilder { ... }`     → ctor() implícito
            //   ✓ `public readonly struct ShadowLine { ... }`      → ctor() implícito
            //   ✓ `public readonly record struct TokenMatch { ... }` → miembros record
            //
            // No cubre (correcto):
            //   ✗ `TriggerTransitionBuilder` tiene un ctor `internal` explícito →
            //     Roslyn no genera ctor implícito adicional, y el ctor explícito
            //     se filtra en IsPubliclyVisible (es internal).
            if (member.IsImplicitlyDeclared)
            {
                bool isImplicitDefaultCtor =
                    member is IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: 0 }
                    && type.TypeKind is TypeKind.Class or TypeKind.Struct;

                // Para tipos record, todos los miembros implícitos se dejan pasar:
                // el analizador RS0016 los exige en PublicAPI.txt y Roslyn puede marcar
                // como IsImplicitlyDeclared cualquier subconjunto de ellos dependiendo
                // de la versión. Los filtros IsPubliclyVisible + ShouldSkipMember se
                // encargan de descartar lo que no debe aparecer (PrintMembers protected,
                // EqualityContract, accesores de propiedad, etc.).
                bool isRecordType = type.IsRecord;

                if (!isImplicitDefaultCtor && !isRecordType) continue;
            }

            // Ignorar override de Object.Finalize, operators implícitos, etc.
            if (ShouldSkipMember(member)) continue;

            IEnumerable<string>? formatted = SymbolFormatter.FormatMember(member);
            if (formatted is not null)
                result.AddRange(formatted);
        }

        // Tipos anidados
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
            CollectFromType(nested, ownAssembly, result);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static bool IsPubliclyVisible(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility is
            Accessibility.Public or
            Accessibility.Protected or
            Accessibility.ProtectedOrInternal;  // protected internal
    }

    private static bool ShouldSkipMember(ISymbol member)
    {
        // Saltar destructores (Finalize)
        if (member is IMethodSymbol { MethodKind: MethodKind.Destructor }) return true;

        // Saltar métodos de acceso de propiedades/eventos (se formatean por separado)
        if (member is IMethodSymbol { AssociatedSymbol: IPropertySymbol or IEventSymbol }) return true;

        // Saltar el método especial EqualityContract de records
        if (member is IPropertySymbol { Name: "EqualityContract" }) return true;

        // Saltar operadores de conversión implícitos de enums
        if (member is IMethodSymbol { MethodKind: MethodKind.BuiltinOperator }) return true;

        return false;
    }
}