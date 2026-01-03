using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Services;
using CdCSharp.BlazorUI.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Component Variants", "BUIButton")]
public class BUIButtonVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        VariantRegistry? registry = ctx.Services.GetRequiredService<IVariantRegistry>() as VariantRegistry;
        BUIButtonVariant customVariant = BUIButtonVariant.Custom("GlassButton");

        registry!.Register<BUIButton, BUIButtonVariant>(
            customVariant,
            button => builder =>
            {
                builder.OpenElement(0, "bui-component");
                builder.AddAttribute(1, "class", "glass-button");
                builder.AddContent(2, button.Text);
                builder.CloseElement();
            });

        // Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Glass Button")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".glass-button").Should().NotBeNull();
        cut.Markup.Should().Contain("Glass Button");
    }
}
