using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Variants", "BUIInputSwitch")]
public class BUIInputSwitchVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>();

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BUIInputSwitchVariant customVariant = BUIInputSwitchVariant.Custom("Pill");

        ctx.Services.AddBlazorUIVariants(builder =>
            builder.ForComponent<BUIInputSwitch>()
                   .AddVariant(
                       customVariant,
                       sw => builder =>
                       {
                           builder.OpenElement(0, "bui-component");
                           builder.AddAttribute(1, "class", "pill-switch");
                           builder.AddContent(2, sw.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Pill Toggle")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".pill-switch").Should().NotBeNull();
        cut.Markup.Should().Contain("Pill Toggle");
    }
}
