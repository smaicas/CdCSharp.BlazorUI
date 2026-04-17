using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Svg;

[Trait("Component Variants", "BUISvgIcon")]
public class BUISvgIconVariantTests
{
    private const string SimpleIcon = "<path d=\"M12 2L2 22h20L12 2z\"/>";

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISvgIcon> cut = ctx.Render<BUISvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUISvgIconVariant custom = BUISvgIconVariant.Custom("outlined");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUISvgIcon>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-outlined-icon");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUISvgIcon> cut = ctx.Render<BUISvgIcon>(p => p
            .Add(c => c.Icon, SimpleIcon)
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-outlined-icon").Should().NotBeNull();
    }
}
