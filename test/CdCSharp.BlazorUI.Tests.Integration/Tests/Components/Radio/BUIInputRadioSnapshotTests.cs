using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Radio;

[Trait("Component Snapshots", "BUIInputRadio")]
public class BUIInputRadioSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Vertical_NoSelection", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")) },

            new { Name = "Vertical_Selected", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.SelectedValue, "opt2")) },

            new { Name = "Horizontal", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Orientation, RadioOrientation.Horizontal)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Disabled, true)) },

            new { Name = "Error", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Error, true)) },

            new { Name = "Required_With_Helper", Builder = (Action<ComponentParameterCollectionBuilder<TestBUIInputRadioConsumer>>)(p => p
                .Add(c => c.Label, "Choice")
                .Add(c => c.Required, true)
                .Add(c => c.HelperText, "You must pick one.")) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
