using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Component Snapshots", "BUIButton")]
public class BUIButtonSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BUIButton>>)(p => p
                .Add(c => c.Text, "Default Button")) },

            new { Name = "WithIcon", Builder = (Action<ComponentParameterCollectionBuilder<BUIButton>>)(p => p
                .Add(c => c.Text, "Icon Button")
                .Add(c => c.LeadingIcon, BUIIcons.MaterialIconsOutlined.i_check)) },

            new { Name = "Loading", Builder = (Action<ComponentParameterCollectionBuilder<BUIButton>>)(p => p
                .Add(c => c.Text, "Loading")
                .Add(c => c.IsLoading, true)
                .Add(c => c.LoadingIndicatorVariant, BUILoadingIndicatorVariant.Spinner)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BUIButton>>)(p => p
                .Add(c => c.Text, "Disabled")
                .Add(c => c.Disabled, true)) },

            new { Name = "Elevated", Builder = (Action<ComponentParameterCollectionBuilder<BUIButton>>)(p => p
                .Add(c => c.Text, "Elevated")
                .Add(c => c.Shadow, BUIShadowPresets.Elevation(8))
                .Add(c => c.Transitions, BUITransitionPresets.HoverLift)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name); ;
    }
}
