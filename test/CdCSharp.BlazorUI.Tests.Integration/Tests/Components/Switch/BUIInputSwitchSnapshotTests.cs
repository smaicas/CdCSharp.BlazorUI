using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Snapshots", "BUIInputSwitch")]
public class BUIInputSwitchSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Off", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Toggle")
                .Add(c => c.Value, false)) },

            new { Name = "On", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Toggle")
                .Add(c => c.Value, true)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Disabled")
                .Add(c => c.Disabled, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Error")
                .Add(c => c.Error, true)) },

            new { Name = "With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Notify")
                .Add(c => c.HelperText, "Send email alerts.")) },

            new { Name = "Large_Compact", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputSwitch>>)(p => p
                .Add(c => c.Label, "Big")
                .Add(c => c.Size, BUISize.Large)
                .Add(c => c.Density, BUIDensity.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
