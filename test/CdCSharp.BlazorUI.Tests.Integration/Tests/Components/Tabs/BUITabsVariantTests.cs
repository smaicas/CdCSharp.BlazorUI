using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Variants", "BUITabs")]
public class BUITabsVariantTests
{
    private static RenderFragment OneTab => b =>
    {
        b.OpenComponent<BUITab>(0);
        b.AddAttribute(1, "Id", "t1");
        b.AddAttribute(2, "Label", "T1");
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Pills_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, BUITabsVariant.Pills));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("pills");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Enclosed_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, BUITabsVariant.Enclosed));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("enclosed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUITabsVariant custom = BUITabsVariant.Custom("segmented");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUITabs>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "bui-component");
                builder.AddAttribute(1, "class", "custom-segmented-tabs");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, OneTab)
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-segmented-tabs").Should().NotBeNull();
    }
}
