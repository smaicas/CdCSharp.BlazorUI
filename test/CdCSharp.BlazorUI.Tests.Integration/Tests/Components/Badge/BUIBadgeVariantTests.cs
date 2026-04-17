using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Variants", "BUIBadge")]
public class BUIBadgeVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUIBadgeVariant custom = BUIBadgeVariant.Custom("pill");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUIBadge>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-pill-badge");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find("span.custom-pill-badge").Should().NotBeNull();
    }
}
