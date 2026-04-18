using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Variants", "BUIToast")]
public class BUIToastVariantTests
{
    private static ToastState DefaultState() => new()
    {
        Content = b => b.AddContent(0, "msg"),
        Options = ToastOptions.Default
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, DefaultState()));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUIToastVariant custom = BUIToastVariant.Custom("Minimal");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUIToast>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-toast-minimal");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, DefaultState())
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-toast-minimal").Should().NotBeNull();
    }
}
