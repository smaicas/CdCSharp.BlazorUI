using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Snapshots", "BUITimePicker")]
public class BUITimePickerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "Default", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => { }) },

            new { Name = "With_Value_14_35", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))) },

            new { Name = "With_Value_09_05", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(9, 5))) },

            new { Name = "Midnight", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(0, 0))) },

            new { Name = "Large_Size", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))
                .Add(c => c.Size, SizeEnum.Large)) },

            new { Name = "Compact_Density", Builder = (Action<ComponentParameterCollectionBuilder<BUITimePicker>>)(p => p
                .Add(c => c.Value, new TimeOnly(14, 35))
                .Add(c => c.Density, DensityEnum.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUITimePicker> cut = ctx.Render<BUITimePicker>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
