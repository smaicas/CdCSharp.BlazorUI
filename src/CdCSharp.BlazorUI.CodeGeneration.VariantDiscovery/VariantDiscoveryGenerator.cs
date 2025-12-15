using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace CdCSharp.BlazorUI.CodeGeneration.VariantDiscovery;

[Generator]
public sealed class VariantTemplateGenerator : IIncrementalGenerator
{
    private const bool EnableDiagnostics = true;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Proveedor de compilación
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        // 2. Descubrir métodos y propiedades con [VariantTemplate]
        IncrementalValuesProvider<TemplateInfo?> candidateTemplates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax or PropertyDeclarationSyntax,
                transform: static (ctx, _) => GetTemplateInfo(ctx))
            .Where(static t => t is not null)!;

        IncrementalValueProvider<ImmutableArray<TemplateInfo?>> combined = compilationProvider.Combine(candidateTemplates.Collect())
            .Select((tuple, _) => tuple.Right);

        // 3. Registrar la salida
        context.RegisterSourceOutput(compilationProvider.Combine(candidateTemplates.Collect()), (SourceProductionContext spc, (Compilation compilation, ImmutableArray<TemplateInfo?> templatesArray) tuple) =>
        {
            Compilation compilation = tuple.compilation;
            List<TemplateInfo> templates = tuple.templatesArray.Where(t => t is not null).Cast<TemplateInfo>().ToList();

            spc.AddSource("AutoRegisterTemplates.g.cs", GenerateSource(templates));

            if (EnableDiagnostics)
            {
                spc.AddSource("AutoRegisterTemplatesDiagnostic.g.cs", GenerateDiagnostics(compilation, templates));
            }
        });
    }

    private static TemplateInfo? GetTemplateInfo(GeneratorSyntaxContext ctx)
    {
        ISymbol? symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node);
        if (symbol is null) return null;

        foreach (AttributeData attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name is not "VariantTemplateAttribute" and not "VariantTemplate") continue;

            if (symbol is IMethodSymbol method) return FromMethod(method, attr);
            if (symbol is IPropertySymbol property) return FromProperty(property, attr);

            return null;
        }

        return null;
    }

    private static TemplateInfo? FromMethod(IMethodSymbol method, AttributeData attr)
    {
        if (method.Parameters.Length != 1 || method.ReturnType.ToDisplayString() != "Microsoft.AspNetCore.Components.RenderFragment")
            return null;

        return CreateTemplateInfo(method, attr, isProperty: false);
    }

    private static TemplateInfo? FromProperty(IPropertySymbol property, AttributeData attr)
    {
        if (property.Type.ToDisplayString() != "Microsoft.AspNetCore.Components.RenderFragment")
            return null;

        return CreateTemplateInfo(property, attr, isProperty: true);
    }

    private static TemplateInfo CreateTemplateInfo(ISymbol member, AttributeData attr, bool isProperty)
    {
        return new TemplateInfo(
            Namespace: member.ContainingType.ContainingNamespace.ToDisplayString(),
            ClassName: member.ContainingType.Name,
            MemberName: member.Name,
            IsProperty: isProperty,
            ComponentType: attr.ConstructorArguments[0].Value?.ToString() ?? "",
            VariantType: attr.ConstructorArguments[1].Value?.ToString() ?? "",
            VariantName: attr.ConstructorArguments[2].Value?.ToString() ?? ""
        );
    }

    private static string GenerateSource(IReadOnlyList<TemplateInfo> templates)
    {
        StringBuilder sb = new();

        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using CdCSharp.BlazorUI.Core.Variants.Services;");
        sb.AppendLine();
        sb.AppendLine("namespace CdCSharp.BlazorUI.Generated;");
        sb.AppendLine();
        sb.AppendLine("public static class AutoRegisterTemplates");
        sb.AppendLine("{");
        sb.AppendLine("    public static void AddGeneratedTemplates(IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine("        var builder = new VariantRegistryBuilder(services);");

        foreach (TemplateInfo t in templates)
        {
            string instanceName = $"__{t.ClassName}";
            sb.AppendLine($"        var {instanceName} = new {t.Namespace}.{t.ClassName}();");
            sb.AppendLine($"        builder.For<{t.ComponentType}, {t.VariantType}>()");
            sb.AppendLine($"               .Register({t.VariantType}.Custom(\"{t.VariantName}\"), component => {instanceName}.{t.MemberName}(component));");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateDiagnostics(Compilation compilation, IReadOnlyList<TemplateInfo> templates)
    {
        StringBuilder sb = new();

        sb.AppendLine("// ===================================================");
        sb.AppendLine("// AUTOGENERATED DIAGNOSTIC FILE - DEBUG PURPOSES");
        sb.AppendLine("// ===================================================");

        sb.AppendLine("// Ensamblados referenciados:");

        foreach (AssemblyIdentity asm in compilation.ReferencedAssemblyNames)
        {
            sb.AppendLine($"// - {asm.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("// Templates encontrados:");

        if (!templates.Any())
        {
            sb.AppendLine("// Ningún template detectado.");
        }
        else
        {
            foreach (TemplateInfo t in templates)
            {
                sb.AppendLine($"// Clase: {t.ClassName}");
                sb.AppendLine($"// Namespace: {t.Namespace}");
                sb.AppendLine($"// Miembro: {t.MemberName}");
                sb.AppendLine($"// Propiedad?: {t.IsProperty}");
                sb.AppendLine($"// Tipo de componente: {t.ComponentType}");
                sb.AppendLine($"// Tipo de variante: {t.VariantType}");
                sb.AppendLine($"// Nombre de la variante: {t.VariantName}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("// ===================================================");
        return sb.ToString();
    }

    private sealed record TemplateInfo(
        string Namespace,
        string ClassName,
        string MemberName,
        bool IsProperty,
        string ComponentType,
        string VariantType,
        string VariantName);
}
