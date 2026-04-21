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
    public const string AttributeShortName = "AutogenerateCssColors";
    public const string AttributeShortNameSuffix = "AutogenerateCssColorsAttribute";

    private readonly record struct NamedColor(string Name, byte R, byte G, byte B, byte A);

    private static readonly NamedColor[] _colors = typeof(Color)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(Color))
        .Select(p =>
        {
            Color c = (Color)p.GetValue(null)!;
            return new NamedColor(p.Name, c.R, c.G, c.B, c.A);
        })
        .ToArray();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassToGenerate> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithAutogenerateCssColorsAttribute(s),
                transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(classDeclarations.Collect(), static (spc, source) => Execute(source, spc));
    }

    private readonly record struct ClassToGenerate(string NamespaceName, string ClassName, int VariantLevels);

    private static ClassToGenerate? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            return null;

        SemanticModel model = context.SemanticModel;

        if (model.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return null;

        foreach (AttributeData attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name == AttributeShortNameSuffix)
            {
                int variantLevels = 5;

                if (attribute.ConstructorArguments.Length > 0 &&
                    attribute.ConstructorArguments[0].Value is int levels &&
                    levels > 0)
                {
                    variantLevels = levels;
                }

                return new ClassToGenerate(
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.Name,
                    variantLevels);
            }
        }

        return null;
    }

    private static bool IsClassWithAutogenerateCssColorsAttribute(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDecl)
            return false;

        foreach (AttributeListSyntax attributeList in classDecl.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                string name = attribute.Name.ToString();
                int lastDot = name.LastIndexOf('.');
                if (lastDot >= 0)
                    name = name.Substring(lastDot + 1);

                if (name == AttributeShortName || name == AttributeShortNameSuffix)
                    return true;
            }
        }

        return false;
    }

    private static void Execute(ImmutableArray<ClassToGenerate> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (ClassToGenerate classToGenerate in classes.Distinct())
        {
            int variantLevels = classToGenerate.VariantLevels;
            string namespaceName = classToGenerate.NamespaceName;
            string className = classToGenerate.ClassName;

            ClassDeclarationSyntax classSyntax = GenerateClassDeclaration(className);

            foreach (NamedColor color in _colors)
            {
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
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("CdCSharp.BlazorUI.Core.Css")),
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