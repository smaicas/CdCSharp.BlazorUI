using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Rendering", "BUIInitializer")]
public class BUIInitializerRenderingTests
{
    internal static readonly Dictionary<string, string> FullPalette = new()
    {
        ["--palette-background"] = "#121212",
        ["--palette-backgroundcontrast"] = "#FFFFFF",
        ["--palette-error"] = "#CF6679",
        ["--palette-errorcontrast"] = "#000000",
        ["--palette-info"] = "#64B5F6",
        ["--palette-infocontrast"] = "#000000",
        ["--palette-primary"] = "#8AB4F8",
        ["--palette-primarycontrast"] = "#000000",
        ["--palette-secondary"] = "#B39DDB",
        ["--palette-secondarycontrast"] = "#000000",
        ["--palette-shadow"] = "#000000",
        ["--palette-success"] = "#81C995",
        ["--palette-successcontrast"] = "#000000",
        ["--palette-surface"] = "#1E1E1E",
        ["--palette-surfacecontrast"] = "#FFFFFF",
        ["--palette-warning"] = "#FFD54F",
        ["--palette-warningcontrast"] = "#000000",
    };

    internal static IThemeJsInterop RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(new ValueTask<Dictionary<string, string>>(FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_After_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>(p => p
            .AddChildContent("<div class='test-child'>Hello</div>"));

        // Assert
        cut.FindAll(".test-child").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_For_Null_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();

        // Assert — no child divs (only HeadContent + CascadingValue shell)
        cut.FindAll("div").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_InitializeAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>(p => p
            .Add(c => c.DefaultTheme, "light"));

        // Assert
        await fake.Received(1).InitializeAsync("light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_GetPaletteAsync_On_First_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();

        // Assert
        await fake.Received(1).GetPaletteAsync();
    }
}
