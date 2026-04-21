using CdCSharp.BlazorUI.Core.CodeGeneration.Tests.Infrastructure;

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
