using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Variants", "BUIInputCheckbox")]
public class BUIInputCheckboxVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>();

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        BUIInputCheckboxVariant customVariant = BUIInputCheckboxVariant.Custom("Toggle");

        ctx.Services.AddBlazorUIVariants(builder =>
            builder.ForComponent<BUIInputCheckbox<bool>>()
                   .AddVariant(
                       customVariant,
                       cb => builder =>
                       {
                           builder.OpenElement(0, "bui-component");
                           builder.AddAttribute(1, "class", "toggle-checkbox");
                           builder.AddContent(2, cb.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Custom Toggle")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".toggle-checkbox").Should().NotBeNull();
        cut.Markup.Should().Contain("Custom Toggle");
    }
}
