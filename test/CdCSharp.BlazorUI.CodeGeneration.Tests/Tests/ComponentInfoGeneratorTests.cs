using CdCSharp.BlazorUI.CodeGeneration.Tests.Infrastructure;
using CdCSharp.BlazorUI.Generator;

namespace CdCSharp.BlazorUI.CodeGeneration.Tests.Tests;

[Trait("Generator Snapshots", "ComponentInfoGenerator")]
public class ComponentInfoGeneratorTests
{
    private const string ParameterAttrSource = """
        namespace Microsoft.AspNetCore.Components;

        [System.AttributeUsage(System.AttributeTargets.Property)]
        public sealed class ParameterAttribute : System.Attribute { }
        """;

    [Fact]
    public async Task Should_Skip_Razor_Without_GenerateComponentInfo_Attribute()
    {
        string razor = """
            @namespace TestNs

            @code {
                [Parameter] public string? Text { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/NoMark.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Generate_From_Basic_Razor()
    {
        string razor = """
            @namespace TestNs
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Visible label.</summary>
                [Parameter] public string? Text { get; set; }

                /// <summary>Disables interaction.</summary>
                [Parameter] public bool Disabled { get; set; } = false;

                [Parameter] public int Count { get; set; } = 42;
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts: [("/src/MyComponent.razor", razor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Inherited_Params_From_Razor_Base()
    {
        string baseRazor = """
            @namespace TestNs

            @code {
                /// <summary>Shared size.</summary>
                [Parameter] public string? Size { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits BaseThing
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Own text.</summary>
                [Parameter] public string? Text { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/BaseThing.razor", baseRazor),
                ("/src/Derived.razor", derivedRazor),
            ]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Resolve_Inherited_Params_From_CSharp_Base()
    {
        string csBase = """
            using Microsoft.AspNetCore.Components;

            namespace TestNs;

            public class CsBase
            {
                /// <summary>From C# base.</summary>
                [Parameter] public string? Shared { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits CsBase
            @attribute [GenerateComponentInfo]

            @code {
                [Parameter] public string? Own { get; set; }
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource, csBase],
            additionalTexts: [("/src/Derived.razor", derivedRazor)]);

        await Verify(output);
    }

    [Fact]
    public async Task Should_Deduplicate_Child_Overrides_Of_Base_Params()
    {
        string baseRazor = """
            @namespace TestNs

            @code {
                /// <summary>Base desc.</summary>
                [Parameter] public string? Text { get; set; }
            }
            """;

        string derivedRazor = """
            @namespace TestNs
            @inherits BaseThing
            @attribute [GenerateComponentInfo]

            @code {
                /// <summary>Child desc.</summary>
                [Parameter] public string? Text { get; set; } = "override";
            }
            """;

        string output = GeneratorTestHarness.Run(
            new ComponentInfoGenerator(),
            sources: [ParameterAttrSource],
            additionalTexts:
            [
                ("/src/BaseThing.razor", baseRazor),
                ("/src/Derived.razor", derivedRazor),
            ]);

        await Verify(output);
    }
}
