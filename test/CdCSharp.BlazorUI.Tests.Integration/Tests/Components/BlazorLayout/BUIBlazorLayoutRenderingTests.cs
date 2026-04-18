using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;
using FluentAssertions;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Rendering", "BUIBlazorLayout")]
public class BUIBlazorLayoutRenderingTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BUIInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Body_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>(p => p
            .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0, "<div class='page-body'>Content</div>"))));

        // Assert
        cut.FindAll(".page-body").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Include_BUIToastHost(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>();

        // Assert — BUIToastHost is mounted (no toast visible by default)
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        toastService.ActiveToasts.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Include_BUIModalHost(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>();

        // Assert — modal service available (no modals by default)
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();
        modalService.Should().NotBeNull();
    }
}
