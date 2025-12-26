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
    public const string AttributeNameContains = "AutogenerateCssColors";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassToGenerate> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithAutogenerateCssColorsAttribute(s),
                transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!); // Convert ClassToGenerate? to ClassToGenerate

        IncrementalValueProvider<(Compilation Left, ImmutableArray<ClassToGenerate> Right)> compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private record ClassToGenerate(INamedTypeSymbol Symbol, int VariantLevels);

    private static ClassToGenerate? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        SemanticModel model = context.SemanticModel;

        if (model.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        foreach (AttributeData attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name.Contains(AttributeNameContains) == true)
            {
                // Obtener el valor del parámetro variantLevels, default 5
                int variantLevels = 5;

                if (attribute.ConstructorArguments.Length > 0 &&
                    attribute.ConstructorArguments[0].Value is int levels &&
                    levels > 0)
                {
                    variantLevels = levels;
                }

                return new ClassToGenerate(classSymbol, variantLevels);
            }
        }

        return null;
    }

    private static bool IsClassWithAutogenerateCssColorsAttribute(SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax classDecl &&
               classDecl.AttributeLists
                   .SelectMany(al => al.Attributes)
                   .Any(a => a.Name.ToString().Contains(AttributeNameContains));
    }

    private void Execute(Compilation compilation, ImmutableArray<ClassToGenerate> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (ClassToGenerate classToGenerate in classes.Distinct())
        {
            INamedTypeSymbol classSymbol = classToGenerate.Symbol;
            int variantLevels = classToGenerate.VariantLevels;

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            PropertyInfo[] colors = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(Color))
                .ToArray();

            ClassDeclarationSyntax classSyntax = GenerateClassDeclaration(className);

            foreach (PropertyInfo color in colors)
            {
                string propertyName = color.Name;
                ClassDeclarationSyntax innerClassDeclaration = GenerateInnerClassDeclaration(propertyName);

                // Default property
                PropertyDeclarationSyntax propertyDeclaration = GenerateColorProperty(propertyName);
                innerClassDeclaration = innerClassDeclaration.AddMembers(propertyDeclaration);

                // Darken variants
                for (int i = 1; i <= variantLevels; i++)
                {
                    PropertyDeclarationSyntax variantDarkenPropertyDeclaration =
                        GenerateVariantPropertyDarken(propertyName, i);
                    innerClassDeclaration = innerClassDeclaration.AddMembers(variantDarkenPropertyDeclaration);
                }

                // Lighten variants
                for (int i = 1; i <= variantLevels; i++)
                {
                    PropertyDeclarationSyntax variantLightenPropertyDeclaration =
                        GenerateVariantPropertyLighten(propertyName, i);
                    innerClassDeclaration = innerClassDeclaration.AddMembers(variantLightenPropertyDeclaration);
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
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Drawing")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Diagnostics.CodeAnalysis"))
                ).AddMembers(namespaceDeclaration
                    .AddMembers(classSyntax)
                );

            string sourceCode = compilationUnit.NormalizeWhitespace().ToFullString();

            context.AddSource($"{className}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private ClassDeclarationSyntax GenerateClassDeclaration(string className) =>
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

    private PropertyDeclarationSyntax GenerateColorProperty(string name)
    {
        string cssColor = $"new CssColor(Color.{name}, null)";

        return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("CssColor"), "Default")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(cssColor)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private ClassDeclarationSyntax GenerateInnerClassDeclaration(string name) =>
        SyntaxFactory.ClassDeclaration(name)
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword));

    private PropertyDeclarationSyntax GenerateVariantPropertyDarken(string name, int index)
    {
        string cssColor = $"new CssColor(Color.{name}, CssColorVariant.Darken({index}))";

        return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName("CssColor"), $"Darken{index}")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(cssColor)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private PropertyDeclarationSyntax GenerateVariantPropertyLighten(string name, int index)
    {
        string cssColor = $"new CssColor(Color.{name}, CssColorVariant.Lighten({index}))";

        return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("CssColor"), $"Lighten{index}")
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(
                                SyntaxFactory.Token(SyntaxKind.SemicolonToken)))))
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(cssColor)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }
}