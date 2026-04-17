using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Loading;

[Trait("Component Variants", "BUILoadingIndicator")]
public class BUILoadingIndicatorVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dots_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.Dots));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("dots");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Linear_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("linearindeterminate");
        cut.Find(".bui-loading-linear").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUILoadingIndicatorVariant custom = BUILoadingIndicatorVariant.Custom("pulse");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUILoadingIndicator>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "bui-component");
                builder.AddAttribute(1, "class", "custom-pulse-loader");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-pulse-loader").Should().NotBeNull();
    }
}
