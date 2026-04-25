using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace GeneratePublicApi;
/// <summary>
/// Formatea los símbolos de Roslyn con el formato exacto que espera
/// Microsoft.CodeAnalysis.PublicApiAnalyzers en sus ficheros .txt.
///
/// Formato de referencia (comportamiento del analizador RS0016/RS0017):
/// <code>
/// #nullable enable
/// MyLib.MyClass
/// MyLib.MyClass.MyClass() -> void
/// static MyLib.MyClass.Create(string! name) -> MyLib.MyClass!
/// MyLib.MyClass.Method(System.Action&lt;MyLib.MyOtherClass!&gt;? cb = null) -> string!
/// MyLib.MyClass.Property.get -> string!
/// MyLib.MyClass.Property.set -> void
/// readonly MyLib.MyStruct.Field -> int
/// const MyLib.MyClass.Constant -> int
/// MyLib.MyEnum
/// MyLib.MyEnum.Value1 = 0 -> MyLib.MyEnum
/// </code>
/// </summary>
internal static class SymbolFormatter
{
    // ── Nullable context ────────────────────────────────────────────────────
    //
    // MSBuildWorkspace no siempre propaga el contexto nullable de cada fichero,
    // dejando NullableAnnotation.None en tipos que en realidad son no-nullable.
    // TypeStr() usa este flag para añadir '!' en esos casos.

    private static bool _nullableEnable = true;

    /// <summary>
    /// Debe llamarse antes de formatear, con el mismo valor que
    /// <c>CliOptions.NullableEnable</c>.
    /// </summary>
    public static void SetNullableEnable(bool value) => _nullableEnable = value;

    // ── Formatos de display ─────────────────────────────────────────────────

    /// <summary>
    /// Usado solo en <see cref="FormatType"/> para la línea de declaración del tipo
    /// (p.ej. <c>MyNs.MyClass&lt;T&gt;</c>). Incluye parámetros de tipo genérico.
    /// </summary>
    private static readonly SymbolDisplayFormat TypeDeclFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    /// <summary>
    /// Usado internamente en <see cref="TypeStr"/> para obtener el nombre base de
    /// un tipo sin parámetros genéricos ni anotaciones nullable (los añadimos
    /// nosotros de forma recursiva).
    /// </summary>
    private static readonly SymbolDisplayFormat TypeBaseFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    // ── API pública ─────────────────────────────────────────────────────────

    public static string FormatType(INamedTypeSymbol type)
        => type.ToDisplayString(TypeDeclFormat);

    public static IEnumerable<string>? FormatMember(ISymbol member) => member switch
    {
        IMethodSymbol m => FormatMethod(m),
        IPropertySymbol p => FormatProperty(p),
        IFieldSymbol f => FormatField(f),
        IEventSymbol e => FormatEvent(e),
        _ => null
    };

    // ── Métodos ─────────────────────────────────────────────────────────────

    private static IEnumerable<string> FormatMethod(IMethodSymbol method)
    {
        if (method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet
                              or MethodKind.EventAdd or MethodKind.EventRemove)
            yield break;

        string owner = method.ContainingType.ToDisplayString(TypeDeclFormat);

        string name = method.MethodKind switch
        {
            MethodKind.Constructor => method.ContainingType.Name,
            // El prefijo "static" para el constructor estático va en el nombre,
            // no en el prefijo de línea, para mantener el formato del analizador.
            MethodKind.StaticConstructor => $"static {method.ContainingType.Name}",
            MethodKind.Conversion when method.Name == "op_Explicit"
                                         => $"explicit operator {TypeStr(method.ReturnType)}",
            MethodKind.Conversion when method.Name == "op_Implicit"
                                         => $"implicit operator {TypeStr(method.ReturnType)}",
            MethodKind.UserDefinedOperator => $"operator {OperatorToken(method.Name)}",
            _ => method.Name
        };

        string typeParams = method.TypeParameters.Length > 0
            ? $"<{string.Join(", ", method.TypeParameters.Select(tp => tp.Name))}>"
            : "";

        // Los overrides sintetizados de Object en tipos record usan la convención
        // especial "~override" del analizador RS0016/RS0017 y sus firmas NO llevan
        // anotaciones nullable (ni '!' ni '?'). Esto afecta a Equals(object),
        // ToString() y GetHashCode() cuando provienen del compilador vía el record.
        //
        // Por qué recorremos la cadena completa de OverriddenMethod:
        //   · record class  → sobreescribe directamente Object.Equals / Object.ToString
        //                      → OverriddenMethod.ContainingType = System.Object  ✓
        //   · record struct → sobreescribe ValueType.Equals / ValueType.ToString,
        //                      que a su vez sobreescribe Object
        //                      → OverriddenMethod.ContainingType = System.ValueType,
        //                        no System.Object  ✗  (era el fallo anterior)
        // Recorriendo la cadena llegamos siempre a Object en ambos casos.
        bool isSynthesizedObjectOverride =
            method.IsOverride &&
            method.ContainingType.IsRecord &&
            OverridesObjectMember(method);

        string parameters = isSynthesizedObjectOverride
            ? BuildParameterListBare(method.Parameters)
            : BuildParameterList(method.Parameters);

        string returnType = method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor
            ? "void"
            : isSynthesizedObjectOverride
                ? method.ReturnType.ToDisplayString(TypeBaseFormat)
                : TypeStr(method.ReturnType);

        // Prefijo de modificador (abstract / virtual / override / sealed override).
        // Las interfaces no llevan prefijo: sus miembros son implícitamente abstract.
        string modifierPrefix = "";
        bool inInterface = method.ContainingType.TypeKind == TypeKind.Interface;

        if (method.MethodKind == MethodKind.Ordinary && !inInterface)
        {
            if (isSynthesizedObjectOverride) modifierPrefix = "~override ";
            else if (method.IsAbstract) modifierPrefix = "abstract ";
            else if (method.IsOverride && method.IsSealed) modifierPrefix = "sealed override ";
            else if (method.IsOverride) modifierPrefix = "override ";
            else if (method.IsVirtual) modifierPrefix = "virtual ";
        }

        // Prefijo "static":
        //   · Constructores estáticos → ya llevan "static" embebido en `name`.
        //   · Conversion / UserDefinedOperator → SÍ llevan "static" (analizador lo exige).
        bool addStaticPrefix = method.IsStatic
            && method.MethodKind != MethodKind.StaticConstructor;

        string prefix = modifierPrefix + (addStaticPrefix ? "static " : "");

        yield return $"{prefix}{owner}.{name}{typeParams}({parameters}) -> {returnType}";
    }

    // ── Propiedades ─────────────────────────────────────────────────────────

    private static IEnumerable<string> FormatProperty(IPropertySymbol prop)
    {
        string owner = prop.ContainingType.ToDisplayString(TypeDeclFormat);
        string propName = prop.IsIndexer
            ? $"this[{BuildParameterList(prop.Parameters)}]"
            : prop.Name;
        string propType = TypeStr(prop.Type);

        // Modifier prefix (abstract / virtual / override / sealed override).
        // Las interfaces no llevan prefijo: sus miembros son implícitamente abstract.
        bool inInterface = prop.ContainingType.TypeKind == TypeKind.Interface;
        string modifierPrefix = "";
        if (!inInterface)
        {
            if (prop.IsAbstract) modifierPrefix = "abstract ";
            else if (prop.IsOverride && prop.IsSealed) modifierPrefix = "sealed override ";
            else if (prop.IsOverride) modifierPrefix = "override ";
            else if (prop.IsVirtual) modifierPrefix = "virtual ";
        }

        // Las propiedades estáticas llevan prefijo "static " igual que los campos/métodos.
        string prefix = modifierPrefix + (prop.IsStatic ? "static " : "");

        if (IsVisible(prop.GetMethod))
            yield return $"{prefix}{owner}.{propName}.get -> {propType}";

        if (IsVisible(prop.SetMethod))
        {
            string setKind = prop.SetMethod!.IsInitOnly ? "init" : "set";
            yield return $"{prefix}{owner}.{propName}.{setKind} -> void";
        }
    }

    // ── Campos ──────────────────────────────────────────────────────────────

    private static IEnumerable<string> FormatField(IFieldSymbol field)
    {
        string owner = field.ContainingType.ToDisplayString(TypeDeclFormat);
        string fieldType = TypeStr(field.Type);

        if (field.ContainingType.TypeKind == TypeKind.Enum)
        {
            // Los valores de enum no llevan prefijo
            yield return $"{owner}.{field.Name} = {field.ConstantValue} -> {owner}";
        }
        else if (field.IsConst)
        {
            // Los campos const incluyen su valor literal, igual que los valores de enum.
            // Ejemplo: const MyNs.MyClass.Pi = 3.14 -> double
            //          const MyNs.MyClass.Name = "foo" -> string!
            string constValue = RenderConstantValue(field.ConstantValue, field.Type);
            yield return $"const {owner}.{field.Name} = {constValue} -> {fieldType}";
        }
        else
        {
            // Emitir prefijos static readonly / static / readonly según corresponda.
            string prefix = (field.IsStatic ? "static " : "")
                          + (field.IsReadOnly ? "readonly " : "");
            yield return $"{prefix}{owner}.{field.Name} -> {fieldType}";
        }
    }

    // ── Eventos ─────────────────────────────────────────────────────────────

    private static IEnumerable<string> FormatEvent(IEventSymbol evt)
    {
        string owner = evt.ContainingType.ToDisplayString(TypeDeclFormat);

        // Field-like event (sin accesores explícitos): el compilador genera
        // add/remove implícitos. PublicApiAnalyzer espera la forma corta
        // "Owner.Name -> EventType" en vez de "Owner.Name.add -> void".
        bool isFieldLike =
            evt.AddMethod?.IsImplicitlyDeclared == true
            && evt.RemoveMethod?.IsImplicitlyDeclared == true;

        if (isFieldLike)
        {
            string evtType = TypeStr(evt.Type);
            string staticPrefix = evt.IsStatic ? "static " : "";
            yield return $"{staticPrefix}{owner}.{evt.Name} -> {evtType}";
            yield break;
        }

        if (IsVisible(evt.AddMethod))
            yield return $"{owner}.{evt.Name}.add -> void";

        if (IsVisible(evt.RemoveMethod))
            yield return $"{owner}.{evt.Name}.remove -> void";
    }

    // ── Parámetros ──────────────────────────────────────────────────────────

    private static string BuildParameterList(ImmutableArray<IParameterSymbol> parameters)
    {
        if (parameters.IsEmpty) return "";
        return string.Join(", ", parameters.Select((p, idx) => FormatParameter(p, idx == 0)));
    }

    /// <summary>
    /// Versión sin anotaciones nullable, usada para los parámetros de los métodos
    /// sintetizados "~override" (Equals(object), ToString) cuya firma es pre-nullable.
    /// </summary>
    private static string BuildParameterListBare(ImmutableArray<IParameterSymbol> parameters)
    {
        if (parameters.IsEmpty) return "";
        return string.Join(", ", parameters.Select(p =>
        {
            StringBuilder sb = new();
            switch (p.RefKind)
            {
                case RefKind.Ref: sb.Append("ref "); break;
                case RefKind.Out: sb.Append("out "); break;
                case RefKind.In: sb.Append("in "); break;
            }
            if (p.IsParams) sb.Append("params ");
            // Tipo sin anotaciones nullable (TypeBaseFormat no incluye ! ni ?)
            sb.Append(p.Type.ToDisplayString(TypeBaseFormat));
            sb.Append(' ');
            sb.Append(p.Name);
            // Sin valor por defecto para los métodos sintetizados de Object
            return sb.ToString();
        }));
    }

    private static string FormatParameter(IParameterSymbol p, bool isFirstOfMethod = false)
    {
        StringBuilder sb = new();

        // Extension method receiver: el primer parámetro lleva 'this' cuando
        // el método contenedor es estático y está marcado como extension.
        if (isFirstOfMethod
            && p.ContainingSymbol is IMethodSymbol { IsExtensionMethod: true })
        {
            sb.Append("this ");
        }

        switch (p.RefKind)
        {
            case RefKind.Ref: sb.Append("ref "); break;
            case RefKind.Out: sb.Append("out "); break;
            case RefKind.In: sb.Append("in "); break;
        }
        if (p.IsParams) sb.Append("params ");

        sb.Append(TypeStr(p.Type));
        sb.Append(' ');
        sb.Append(p.Name);

        if (p.HasExplicitDefaultValue)
        {
            sb.Append(" = ");
            sb.Append(RenderDefaultValue(p));
        }

        return sb.ToString();
    }

    private static string RenderDefaultValue(IParameterSymbol p)
    {
        if (p.ExplicitDefaultValue is null) return "null";
        return RenderConstantValue(p.ExplicitDefaultValue, p.Type);
    }

    /// <summary>
    /// Convierte el valor constante de un campo o parámetro al literal que escribe
    /// el analizador RS0016 en el fichero .txt.
    ///
    /// <list type="bullet">
    ///   <item>Enums      → <c>Ns.EnumType.MemberName</c>  (nunca el entero)</item>
    ///   <item>Strings    → <c>"valor"</c> con escape de <c>\</c>, <c>"</c>, etc.</item>
    ///   <item>Bools      → <c>true</c> / <c>false</c></item>
    ///   <item>Numéricos  → InvariantCulture (punto decimal)</item>
    ///   <item>null       → <c>null</c></item>
    /// </list>
    /// </summary>
    private static string RenderConstantValue(object? value, ITypeSymbol type)
    {
        if (value is null) return "null";

        // Valores de enums: "Ns.EnumType.MemberName" en lugar del entero bruto.
        if (type.TypeKind == TypeKind.Enum)
        {
            INamedTypeSymbol enumType = (INamedTypeSymbol)type;
            string qualifiedEnumName = enumType.ToDisplayString(TypeBaseFormat);
            foreach (IFieldSymbol f in enumType.GetMembers().OfType<IFieldSymbol>())
            {
                if (f.ConstantValue?.Equals(value) == true)
                    return $"{qualifiedEnumName}.{f.Name}";
            }
            // Fallback al entero para flags combinados o valores no representables.
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default";
        }

        // Usar InvariantCulture para que float/double siempre usen '.'
        // como separador decimal (en locales como es-ES ToString() da "0,2").
        // Las cadenas necesitan escape de backslash y comillas para que el fichero
        // .txt sea legible (p.ej. el valor "\" debe salir como "\\").
        return value switch
        {
            string s => $"\"{EscapeString(s)}\"",
            bool b => b ? "true" : "false",
            char c => $"'{c}'",
            float f => f.ToString("G", CultureInfo.InvariantCulture),
            double d => d.ToString("G", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "default"
        };
    }

    // ── TypeStr: formateador recursivo de tipos con nullable ─────────────────
    //
    // Por qué no basta con ToDisplayString(TypeFormat con IncludeNullableReferenceTypeModifier):
    //
    //   MSBuildWorkspace deja frecuentemente NullableAnnotation.None en tipos que
    //   en el código fuente son no-nullable (string, MyClass, etc.). Roslyn solo
    //   añade '!' cuando la anotación es NotAnnotated, no cuando es None.
    //
    // Por qué necesitamos recursión y no solo AppendNullability en el tipo raíz:
    //
    //   Para `Action<TransitionTiming>?` el tipo raíz tiene NullableAnnotation.Annotated
    //   (de ahí el '?'), así que no añadiríamos '!'. Pero el argumento de tipo
    //   TransitionTiming sí tiene NullableAnnotation.None y necesita '!'.
    //   Sin recursión obtenemos `Action<TransitionTiming>?` en vez de
    //   `Action<TransitionTiming!>?`.

    /// <summary>
    /// Convierte un <see cref="ITypeSymbol"/> a la representación de texto que
    /// espera PublicApiAnalyzers, añadiendo '!' y '?' de forma recursiva en
    /// todos los niveles de anidamiento genérico.
    /// </summary>
    private static string TypeStr(ITypeSymbol type)
    {
        // ── Nullable value type: Nullable<T> → T? ──────────────────────────
        // Hay que detectarlo antes del caso genérico general para no emitir
        // "System.Nullable<int>" sino simplemente "int?".
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableVal)
            return TypeStr(nullableVal.TypeArguments[0]) + "?";

        // ── Array ──────────────────────────────────────────────────────────
        if (type is IArrayTypeSymbol array)
        {
            string elemStr = TypeStr(array.ElementType);
            // Rank > 1 → "int[,]", "int[,,]", etc.
            string brackets = "[" + new string(',', array.Rank - 1) + "]";
            return AppendNullability(array, elemStr + brackets);
        }

        // ── Tipo genérico construido (List<T>, Action<T1,T2>, ...) ─────────
        // Construimos el nombre base + argumentos de tipo recursivamente para
        // que cada argumento también reciba su '!' si corresponde.
        if (type is INamedTypeSymbol { IsGenericType: true } generic)
        {
            // OriginalDefinition da el tipo abierto (List<T>); con TypeBaseFormat
            // (genericsOptions: None) obtenemos solo el nombre cualificado base,
            // p.ej. "System.Collections.Generic.List" o "System.Action".
            string baseName = generic.OriginalDefinition.ToDisplayString(TypeBaseFormat);
            string args = string.Join(", ", generic.TypeArguments.Select(TypeStr));
            return AppendNullability(type, $"{baseName}<{args}>");
        }

        // ── Parámetro de tipo (T, TKey, TValue, ...) ──────────────────────
        // El analizador SÍ anota type parameters cuando el contexto nullable
        // está habilitado y el parámetro es referencia (constraint `class`,
        // `notnull`, o constraint a un tipo de referencia).
        if (type is ITypeParameterSymbol tp)
            return AppendNullability(tp, tp.Name);

        // ── Tipo simple (string, int, bool, MyClass, MyStruct, …) ─────────
        // TypeBaseFormat: sin generics ni nullable, solo nombre cualificado.
        return AppendNullability(type, type.ToDisplayString(TypeBaseFormat));
    }

    /// <summary>
    /// Añade '?' o '!' al string <paramref name="core"/> según la
    /// <see cref="NullableAnnotation"/> del tipo y el valor de
    /// <see cref="_nullableEnable"/>.
    /// </summary>
    private static string AppendNullability(ITypeSymbol type, string core)
    {
        // Tipo referencia explícitamente nullable (string?, MyClass?, Action<T>?)
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return core.EndsWith('?') ? core : core + "?";

        // Tipo referencia no-nullable (o con anotación desconocida que tratamos
        // como no-nullable cuando nullable está habilitado en el proyecto).
        if (_nullableEnable && type.IsReferenceType
            && type.NullableAnnotation is NullableAnnotation.NotAnnotated
                                       or NullableAnnotation.None)
            return core.EndsWith('!') ? core : core + "!";

        return core;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static bool IsVisible(IMethodSymbol? method)
        => method is
        {
            DeclaredAccessibility: Accessibility.Public
                                            or Accessibility.Protected
                                            or Accessibility.ProtectedOrInternal
        };

    private static string OperatorToken(string opName) => opName switch
    {
        "op_Addition" => "+",
        "op_Subtraction" => "-",
        "op_Multiply" => "*",
        "op_Division" => "/",
        "op_Modulus" => "%",
        "op_Equality" => "==",
        "op_Inequality" => "!=",
        "op_LessThan" => "<",
        "op_GreaterThan" => ">",
        "op_LessThanOrEqual" => "<=",
        "op_GreaterThanOrEqual" => ">=",
        "op_BitwiseAnd" => "&",
        "op_BitwiseOr" => "|",
        "op_ExclusiveOr" => "^",
        "op_LeftShift" => "<<",
        "op_RightShift" => ">>",
        "op_UnaryNegation" => "-",
        "op_UnaryPlus" => "+",
        "op_LogicalNot" => "!",
        "op_OnesComplement" => "~",
        "op_Increment" => "++",
        "op_Decrement" => "--",
        "op_True" => "true",
        "op_False" => "false",
        _ => opName
    };

    /// <summary>
    /// Escapa los caracteres especiales de una cadena para que sea legible en
    /// el fichero PublicAPI.txt del mismo modo que lo hace el compilador de C#.
    /// Ejemplo: "\" (barra invertida) → "\\".
    /// </summary>
    private static string EscapeString(string s)
        => s.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

    /// <summary>
    /// Devuelve <c>true</c> si <paramref name="method"/> sobreescribe, en cualquier
    /// punto de la cadena de herencia, un método declarado en <c>System.Object</c>.
    ///
    /// <para>
    /// Es necesario recorrer toda la cadena porque los <c>record struct</c> sobreescriben
    /// primero <c>System.ValueType</c> (que es quien ya sobreescribe <c>System.Object</c>),
    /// por lo que <c>method.OverriddenMethod.ContainingType</c> apunta a <c>ValueType</c>,
    /// no directamente a <c>Object</c>.
    /// </para>
    /// </summary>
    private static bool OverridesObjectMember(IMethodSymbol method)
    {
        IMethodSymbol? current = method.OverriddenMethod;
        while (current is not null)
        {
            if (current.ContainingType.SpecialType == SpecialType.System_Object)
                return true;
            current = current.OverriddenMethod;
        }
        return false;
    }
}