using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Snapshots", "BUIThemeSelector")]
public class BUIThemeSelectorSnapshotTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string theme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(theme));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_ThemeSelector_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        (string Name, Action<ComponentParameterCollectionBuilder<BUIThemeSelector>> Builder)[] testCases =
        [
            ("Default_Light", p => p
                .Add(c => c.Variant, BUIThemeSelectorVariant.Default)
                .Add(c => c.ShowIcon, true)),
            ("Default_NoIcon", p => p
                .Add(c => c.Variant, BUIThemeSelectorVariant.Default)
                .Add(c => c.ShowIcon, false)),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
