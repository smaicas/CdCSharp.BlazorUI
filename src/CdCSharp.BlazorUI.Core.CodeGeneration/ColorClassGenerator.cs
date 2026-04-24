// CdCSharp.BlazorUI.Core\SourceGenerators\ColorClassGenerator.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CdCSharp.BlazorUI.Core.CodeGeneration;

[Generator]
public class ColorClassGenerator : IIncrementalGenerator
{
    internal static readonly DiagnosticDescriptor MustBePartialStaticRule = new(
        id: "BUIGEN010",
        title: "AutogenerateCssColorsAttribute requires a partial static class",
        messageFormat: "Class '{0}' is decorated with [AutogenerateCssColors] but is not declared as 'public static partial'. Add the missing modifier(s).",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The ColorClassGenerator emits per-color nested classes as a partial extension of the target type; the target must therefore be declared 'static partial' so the generated file can merge with the user-authored source.");

    private readonly record struct NamedColor(string Name, byte R, byte G, byte B, byte A);

    /// <summary>Stable, cache-friendly carrier for a <see cref="Location"/>.</summary>
    internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
    {
        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);
    }

    private static readonly NamedColor[] _colors = typeof(Color)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(Color))
        .Select(p =>
        {
            Color c = (Color)p.GetValue(null)!;
            return new NamedColor(p.Name, c.R, c.G, c.B, c.A);
        })
        .ToArray();

    public const string AttributeMetadataName = "CdCSharp.BlazorUI.Components.AutogenerateCssColorsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassToGenerate> classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeMetadataName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => GetSemanticTargetFromAttribute(ctx, ct))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(classDeclarations.Collect(), static (spc, source) => Execute(source, spc));
    }

    private readonly record struct ClassToGenerate(
        string NamespaceName,
        string ClassName,
        int VariantLevels,
        bool IsStatic,
        bool IsPartial,
        LocationInfo Location);

    private static ClassToGenerate? GetSemanticTargetFromAttribute(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return null;

        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        // ForAttributeWithMetadataName guarantees Attributes contains at least one match.
        AttributeData attribute = context.Attributes[0];

        int variantLevels = 5;
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is int levels &&
            levels > 0)
        {
            variantLevels = levels;
        }

        bool isStatic = classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
        bool isPartial = classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);

        SyntaxTree tree = classDeclaration.SyntaxTree;
        TextSpan span = classDeclaration.Identifier.Span;
        LocationInfo location = new(
            tree.FilePath,
            span,
            tree.GetLineSpan(span).Span);

        return new ClassToGenerate(
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            variantLevels,
            isStatic,
            isPartial,
            location);
    }

    private static void Execute(ImmutableArray<ClassToGenerate> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        CancellationToken ct = context.CancellationToken;

        foreach (ClassToGenerate classToGenerate in classes.Distinct())
        {
            ct.ThrowIfCancellationRequested();

            if (!classToGenerate.IsStatic || !classToGenerate.IsPartial)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MustBePartialStaticRule,
                    classToGenerate.Location.ToLocation(),
                    classToGenerate.ClassName));
                continue;
            }

            int variantLevels = classToGenerate.VariantLevels;
            string namespaceName = classToGenerate.NamespaceName;
            string className = classToGenerate.ClassName;

            ClassDeclarationSyntax classSyntax = GenerateClassDeclaration(className);

            foreach (NamedColor color in _colors)
            {
                ct.ThrowIfCancellationRequested();

                ClassDeclarationSyntax innerClassDeclaration = GenerateInnerClassDeclaration(color.Name);

                string defaultCssColor = $"new CssColor({color.R}, {color.G}, {color.B}, {color.A})";
                innerClassDeclaration = innerClassDeclaration.AddMembers(
                    GenerateProperty("Default", defaultCssColor));

                for (int i = 1; i <= variantLevels; i++)
                {
                    string darkenCssColor = $"new CssColor({color.R}, {color.G}, {color.B}, {color.A}, CssColorVariant.Darken({i}))";
                    innerClassDeclaration = innerClassDeclaration.AddMembers(
                        GenerateProperty($"Darken{i}", darkenCssColor));
                }

                for (int i = 1; i <= variantLevels; i++)
                {
                    string lightenCssColor = $"new CssColor({color.R}, {color.G}, {color.B}, {color.A}, CssColorVariant.Lighten({i}))";
                    innerClassDeclaration = innerClassDeclaration.AddMembers(
                        GenerateProperty($"Lighten{i}", lightenCssColor));
                }

                classSyntax = classSyntax.AddMembers(innerClassDeclaration);
            }

            NamespaceDeclarationSyntax namespaceDeclaration =
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
                    .WithNamespaceKeyword(
                        SyntaxFactory.Token(
                            SyntaxFactory.TriviaList(),
                            SyntaxKind.NamespaceKeyword,
                            "namespace",
                            "namespace",
                            SyntaxFactory.TriviaList(SyntaxFactory.Space)));

            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("CdCSharp.BlazorUI.Components")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.CodeAnalysis"))
                ).AddMembers(namespaceDeclaration
                    .AddMembers(classSyntax)
                );

            string sourceCode = compilationUnit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{className}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private static ClassDeclarationSyntax GenerateClassDeclaration(string className) =>
        SyntaxFactory.ClassDeclaration(className)
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("ExcludeFromCodeCoverage"))
                    )
                )
            )
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword));

    private static ClassDeclarationSyntax GenerateInnerClassDeclaration(string name) =>
        SyntaxFactory.ClassDeclaration(name)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword));

    private static PropertyDeclarationSyntax GenerateProperty(string name, string initializer)
    {
        return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("CssColor"), name)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                          SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(initializer)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}