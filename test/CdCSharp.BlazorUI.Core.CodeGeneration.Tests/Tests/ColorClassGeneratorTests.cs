using CdCSharp.BlazorUI.Core.CodeGeneration.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CdCSharp.BlazorUI.Core.CodeGeneration.Tests.Tests;

[Trait("Generator Snapshots", "ColorClassGenerator")]
public class ColorClassGeneratorTests
{
    private const string AttributeSource = """
        using System;

        namespace CdCSharp.BlazorUI.Core;

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class AutogenerateCssColorsAttribute : Attribute
        {
            public AutogenerateCssColorsAttribute() { }
            public AutogenerateCssColorsAttribute(int variantLevels) { }
        }
        """;

    [Fact]
    public async Task Should_Generate_Nothing_When_No_Attribute()
    {
        const string source = """
            namespace TestNs;

            public static partial class Colors { }
            """;

        string output = GeneratorTestHarness.Run(
            new ColorClassGenerator(),
            sources: [AttributeSource, source]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Generate_With_Default_Variant_Levels()
    {
        const string source = """
            using CdCSharp.BlazorUI.Core;

            namespace TestNs;

            [AutogenerateCssColors]
            public static partial class Colors { }
            """;

        string output = GeneratorTestHarness.Run(
            new ColorClassGenerator(),
            sources: [AttributeSource, source]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Generate_With_Custom_Variant_Levels()
    {
        const string source = """
            using CdCSharp.BlazorUI.Core;

            namespace TestNs;

            [AutogenerateCssColors(2)]
            public static partial class Colors { }
            """;

        string output = GeneratorTestHarness.Run(
            new ColorClassGenerator(),
            sources: [AttributeSource, source]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Not_Generate_For_Attribute_With_Colliding_Substring_Name()
    {
        const string collidingAttribute = """
            using System;

            namespace TestNs;

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class FakeAutogenerateCssColorsHelperAttribute : Attribute { }
            """;

        const string source = """
            namespace TestNs;

            [FakeAutogenerateCssColorsHelper]
            public static partial class NotAPalette { }
            """;

        string output = GeneratorTestHarness.Run(
            new ColorClassGenerator(),
            sources: [AttributeSource, collidingAttribute, source]);

        await Verify(output);
    }

    [Fact]
#pragma warning disable xUnit1051
    public void Should_Reuse_Incremental_Cache_When_Source_Unchanged()
    {
        const string source = """
            using CdCSharp.BlazorUI.Core;

            namespace TestNs;

            [AutogenerateCssColors(1)]
            public static partial class Colors { }
            """;

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorCacheTest",
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(AttributeSource, new CSharpParseOptions(LanguageVersion.Latest)),
                CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))
            },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { new ColorClassGenerator().AsSourceGenerator() },
            additionalTexts: null,
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
            optionsProvider: null,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        CSharpCompilation unrelatedEdit = compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace TestNs; public class Unrelated {}",
                new CSharpParseOptions(LanguageVersion.Latest)));

        driver = driver.RunGenerators(unrelatedEdit);
        GeneratorRunResult run = driver.GetRunResult().Results.Single();

        // Arrange
        IEnumerable<IncrementalGeneratorRunStep> outputSteps = run.TrackedOutputSteps.SelectMany(kv => kv.Value);

        // Act
        IEnumerable<IncrementalStepRunReason> outputReasons = outputSteps.SelectMany(s => s.Outputs.Select(o => o.Reason));

        // Assert
        outputReasons.Should().OnlyContain(r => r == IncrementalStepRunReason.Cached || r == IncrementalStepRunReason.Unchanged);
    }
#pragma warning restore xUnit1051

    [Fact]
    public async Task Should_Generate_For_Multiple_Classes()
    {
        const string source = """
            using CdCSharp.BlazorUI.Core;

            namespace TestNs.A
            {
                [AutogenerateCssColors(1)]
                public static partial class PaletteA { }
            }

            namespace TestNs.B
            {
                [AutogenerateCssColors(1)]
                public static partial class PaletteB { }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ColorClassGenerator(),
            sources: [AttributeSource, source]);

        await Verify(output);
    }
}
