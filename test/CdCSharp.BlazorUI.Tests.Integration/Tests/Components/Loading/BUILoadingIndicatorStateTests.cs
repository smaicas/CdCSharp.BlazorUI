using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Loading;

[Trait("Component State", "BUILoadingIndicator")]
public class BUILoadingIndicatorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Aria_Label_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Loading");

        // Act
        cut.Render(p => p.Add(c => c.AriaLabel, "Saving data"));

        // Assert
        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Saving data");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Size, BUISize.Small));

        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("small");

        // Act
        cut.Render(p => p.Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Color, "#ff0000"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Variant_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.Spinner));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("spinner");

        // Act
        cut.Render(p => p.Add(c => c.Variant, BUILoadingIndicatorVariant.Dots));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("dots");
    }
}
