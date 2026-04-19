using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Snapshots", "BUIBlazorLayout")]
public class BUIBlazorLayoutSnapshotTests
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
    public async Task Should_Match_Layout_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        (string Name, Action<ComponentParameterCollectionBuilder<BUIBlazorLayout>> Builder)[] testCases =
        [
            ("EmptyBody", _ => { }),
            ("SimpleBody", p => p
                .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0,
                    "<main class=\"app-main\">Hello</main>")))),
            ("StructuredBody", p => p
                .Add(c => c.Body, (RenderFragment)(b => b.AddMarkupContent(0,
                    "<header class=\"app-header\">H</header>"
                    + "<aside class=\"app-sidebar\">S</aside>"
                    + "<main class=\"app-main\">M</main>")))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verify(results).UseParameters(scenario.Name);
    }
}
