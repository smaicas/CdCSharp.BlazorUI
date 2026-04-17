using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Snapshots", "BUISwitch")]
public class BUISwitchSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Default_Off",
                Html = ctx.Render<BUISwitch<bool>>(p => p
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.InputId, "snap-off")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Default_On",
                Html = ctx.Render<BUISwitch<bool>>(p => p
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, true)
                    .Add(c => c.InputId, "snap-on")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Disabled",
                Html = ctx.Render<BUISwitch<bool>>(p => p
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Disabled, true)
                    .Add(c => c.InputId, "snap-disabled")).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Label",
                Html = ctx.Render<BUISwitch<bool>>(p => p
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Label, "Enable feature")
                    .Add(c => c.InputId, "snap-label")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Custom_Colors",
                Html = ctx.Render<BUISwitch<bool>>(p => p
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.TrackColorActive, "#00aa00")
                    .Add(c => c.TrackColorInactive, "#aaaaaa")
                    .Add(c => c.InputId, "snap-colors")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
