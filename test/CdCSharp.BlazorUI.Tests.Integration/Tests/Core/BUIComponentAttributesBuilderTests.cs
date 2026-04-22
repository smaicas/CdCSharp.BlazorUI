using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.BaseComponents;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core;

/// <summary>
/// CORE-T-04: TypeInfo cache prevents repeated reflection.
/// Each type's IHas* flags are computed once and stored in _typeInfoCache.
/// Verified indirectly: multiple renders of the same type produce identical attribute names.
/// </summary>
[Trait("Core", "BUIComponentAttributesBuilder")]
public class BUIComponentAttributesBuilderTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Same_ComponentType_Should_Produce_Same_Attribute_Names_Across_Instances(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — render same type twice
        IRenderedComponent<BUIComponentBase_TestStub> cut1 = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Size, BUISize.Small));
        IRenderedComponent<BUIComponentBase_TestStub> cut2 = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Size, BUISize.Large));

        // Act — read component name attribute (derived from type cache)
        string? name1 = cut1.Find("div").GetAttribute("data-bui-component");
        string? name2 = cut2.Find("div").GetAttribute("data-bui-component");

        // Assert — same type → same kebab name regardless of instance
        name1.Should().Be(name2);
        name1.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BUI_Prefix_Stripped_In_Cache_Derived_Name(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>();

        // Assert — BUI prefix stripped, CamelCase → kebab (underscore preserved as-is)
        string? name = cut.Find("div").GetAttribute("data-bui-component");
        name.Should().NotBeNullOrEmpty();
        name.Should().NotStartWith("bui");
        name.Should().Contain("component");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Volatile_Flags_From_Cache_Are_Accurate_For_IHasLoading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — component implements IHasLoading → flag is in cache
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Loading, true));

        // Assert — loading attribute present (cache correctly identified IHasLoading)
        cut.Find("div").GetAttribute("data-bui-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task ComputedAttributes_Preserves_UserAttributes_After_PatchVolatile(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Dictionary<string, object> attrs = new() { { "data-testid", "my-component" } };
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.AdditionalAttributes, attrs)
            .Add(c => c.Loading, false));

        // Act — patch volatile attribute
        cut.Render(p => p
            .Add(c => c.AdditionalAttributes, attrs)
            .Add(c => c.Loading, true));

        // Assert — user attribute preserved after patch
        cut.Find("div").GetAttribute("data-testid").Should().Be("my-component");
        cut.Find("div").GetAttribute("data-bui-loading").Should().Be("true");
    }
}
