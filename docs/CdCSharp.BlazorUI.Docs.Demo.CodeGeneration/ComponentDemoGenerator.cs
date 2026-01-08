using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

[Generator]
public class ComponentDemoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<(INamedTypeSymbol pageSymbol, string componentName)?> pagesWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => GetPageWithAttribute(ctx))
            .Where(p => p != null);

        IncrementalValueProvider<(Compilation Left, ImmutableArray<(INamedTypeSymbol pageSymbol, string componentName)?> Right)> compilationAndPages = context.CompilationProvider.Combine(pagesWithAttribute.Collect());

        context.RegisterSourceOutput(compilationAndPages, (spc, source) =>
        {
            (Compilation? compilation, ImmutableArray<(INamedTypeSymbol pageSymbol, string componentName)?> pages) = source;
            foreach ((INamedTypeSymbol pageSymbol, string componentName)? pageOpt in pages!)
            {
                if (pageOpt is null) continue;
                GenerateFiles(spc, compilation!, pageOpt.Value.pageSymbol, pageOpt.Value.componentName);
            }
        });
    }

    private static (INamedTypeSymbol pageSymbol, string componentName)? GetPageWithAttribute(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is ClassDeclarationSyntax cls &&
            ctx.SemanticModel.GetDeclaredSymbol(cls) is INamedTypeSymbol symbol)
        {
            foreach (AttributeData attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.Name == "BuildComponentDemoAttribute" && attr.ConstructorArguments.Length == 1)
                {
                    string name = attr.ConstructorArguments[0].Value?.ToString() ?? "";
                    return (symbol, name);
                }
            }
        }
        return null;
    }

    private static void GenerateFiles(SourceProductionContext context, Compilation compilation, INamedTypeSymbol pageSymbol, string componentName)
    {
        INamedTypeSymbol componentSymbol = compilation.GlobalNamespace.GetNamespaceTypesRecursive()
            .FirstOrDefault(t => t.Name == componentName);
        if (componentSymbol == null) return;

        var parameters = componentSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "ParameterAttribute"))
            .Select(p => new
            {
                Name = p.Name,
                Type = p.Type,
                DefaultValue = p.DeclaringSyntaxReferences
                    .Select(sr => sr.GetSyntax())
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(s => s.Initializer?.Value.ToString())
                    .FirstOrDefault(),
                IsEnum = p.Type.TypeKind == TypeKind.Enum,
                IsBool = p.Type.SpecialType == SpecialType.System_Boolean,
                IsReferenceType = p.Type.IsReferenceType
            })
            .ToList();

        string ns = pageSymbol.ContainingNamespace.ToDisplayString();

        // ===================== Generar Demos =====================
        StringBuilder demosBuilder = new();
        demosBuilder.AppendLine("using Microsoft.AspNetCore.Components;");
        demosBuilder.AppendLine("using CdCSharp.BlazorUI.Components;");
        demosBuilder.AppendLine($"namespace {ns};");
        demosBuilder.AppendLine($"public partial class {componentName}Demos");
        demosBuilder.AppendLine("{");

        List<string> demoNames = [];

        // RenderBasic
        demosBuilder.AppendLine("    public RenderFragment RenderBasic => __builder => {");
        demosBuilder.AppendLine($"        __builder.OpenComponent<{componentName}>(0);");
        var textParam = parameters.FirstOrDefault(p => p.Type.SpecialType == SpecialType.System_String || p.IsReferenceType);
        if (textParam != null)
            demosBuilder.AppendLine($"        __builder.AddAttribute(1, \"{textParam.Name}\", \"Click me\");");
        demosBuilder.AppendLine($"        __builder.CloseComponent();");
        demosBuilder.AppendLine("    };");
        demoNames.Add("RenderBasic");

        int renderIndex = 0;

        // RenderFragments para enums
        foreach (var p in parameters.Where(p => p.IsEnum))
        {
            if (p.Type is not INamedTypeSymbol enumType) continue;

            string renderName = $"Render{p.Name}";
            demoNames.Add(renderName);
            demosBuilder.AppendLine($"    public RenderFragment {renderName} => __builder => {{");
            int i = 0;
            foreach (IFieldSymbol? val in enumType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue))
            {
                demosBuilder.AppendLine($"        __builder.OpenComponent<{componentName}>({i});");
                if (textParam != null)
                    demosBuilder.AppendLine($"        __builder.AddAttribute({i + 1}, \"{textParam.Name}\", \"{val.Name}\");");
                demosBuilder.AppendLine($"        __builder.AddAttribute({i + 2}, \"{p.Name}\", {p.Type.ToDisplayString()}.{val.Name});");
                demosBuilder.AppendLine($"        __builder.CloseComponent();");
                i += 3;
            }
            demosBuilder.AppendLine("    };");
            renderIndex++;
        }

        // RenderFragments para bools
        foreach (var p in parameters.Where(p => p.IsBool))
        {
            string renderName = $"Render{p.Name}";
            demoNames.Add(renderName);
            demosBuilder.AppendLine($"    public RenderFragment {renderName} => __builder => {{");
            int i = 0;
            foreach (bool val in new[] { true, false })
            {
                demosBuilder.AppendLine($"        __builder.OpenComponent<{componentName}>({i});");
                if (textParam != null)
                    demosBuilder.AppendLine($"        __builder.AddAttribute({i + 1}, \"{textParam.Name}\", \"Click me\");");
                demosBuilder.AppendLine($"        __builder.AddAttribute({i + 2}, \"{p.Name}\", {val.ToString().ToLower()});");
                demosBuilder.AppendLine($"        __builder.CloseComponent();");
                i += 3;
            }
            demosBuilder.AppendLine("    };");
        }

        // Información de parámetros
        demosBuilder.AppendLine("    public record ParameterInfo(string Name, Type Type, object? DefaultValue, IEnumerable<object>? Values);");
        demosBuilder.AppendLine("    public static readonly ParameterInfo[] Parameters = new ParameterInfo[] {");
        foreach (var p in parameters)
        {
            string values = "null";
            if (p.IsEnum && p.Type is INamedTypeSymbol enumType)
            {
                values = $"new object[] {{ {string.Join(", ", enumType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).Select(f => $"{p.Type.ToDisplayString()}.{f.Name}"))} }}";
            }
            else if (p.IsBool)
            {
                values = "new object[] { true, false }";
            }

            string typeName;
            if (p.Type.IsReferenceType)
                typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat); // sin ?
            else
                typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            demosBuilder.AppendLine($"        new(\"{p.Name}\", typeof({typeName}), {(p.DefaultValue ?? "null")}, {values}),");
        }
        demosBuilder.AppendLine("    };");

        demosBuilder.AppendLine("}");
        context.AddSource($"{componentName}Demos.g.cs", demosBuilder.ToString());

        // ===================== Generar Doc =====================
        StringBuilder docBuilder = new();
        docBuilder.AppendLine("using CdCSharp.BlazorUI.Docs.Demo.Abstractions;");
        docBuilder.AppendLine($"namespace {ns};");
        docBuilder.AppendLine($"public static class {componentName}Doc");
        docBuilder.AppendLine("{");
        docBuilder.AppendLine("    public static ComponentDocDefinition Definition => new()");
        docBuilder.AppendLine("    {");
        docBuilder.AppendLine($"        Name = \"{componentName}\",");

        IEnumerable<string> interfaces = componentSymbol.AllInterfaces.Select(i => $"\"{i.ToDisplayString()}\"");
        docBuilder.AppendLine($"        Behaviors = {{ {string.Join(", ", interfaces)} }},");

        docBuilder.AppendLine($"        Parameters = new List<ParameterDefinition>({componentName}Demos.Parameters.Select(p => new ParameterDefinition(p.Name, p.Type, p.DefaultValue))),");

        docBuilder.AppendLine("        Demos = new List<ComponentDemoDefinition>");
        docBuilder.AppendLine("        {");
        foreach (string demo in demoNames)
        {
            docBuilder.AppendLine($"            new ComponentDemoDefinition {{ Demo = new {componentName}Demos().{demo}, Code = \"<{componentName} ... />\" }},");
        }
        docBuilder.AppendLine("        }");

        docBuilder.AppendLine("    };");
        docBuilder.AppendLine("}");
        context.AddSource($"{componentName}Doc.g.cs", docBuilder.ToString());
    }
}

internal static class RoslynExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetNamespaceTypesRecursive(this INamespaceSymbol ns)
    {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers())
            yield return type;

        foreach (INamespaceSymbol sub in ns.GetNamespaceMembers())
            foreach (INamedTypeSymbol type in sub.GetNamespaceTypesRecursive())
                yield return type;
    }
}
