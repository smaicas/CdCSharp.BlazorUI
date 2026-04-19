using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Snapshots", "BUIDatePicker")]
public class BUIDatePickerSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new { Name = "With_Value_June_15_2024", Builder = (Action<ComponentParameterCollectionBuilder<BUIDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))) },

            new { Name = "With_Value_January_1_2020", Builder = (Action<ComponentParameterCollectionBuilder<BUIDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2020, 1, 1))) },

            new { Name = "Large_Size", Builder = (Action<ComponentParameterCollectionBuilder<BUIDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.Size, SizeEnum.Large)) },

            new { Name = "Compact_Density", Builder = (Action<ComponentParameterCollectionBuilder<BUIDatePicker>>)(p => p
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.Density, DensityEnum.Compact)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
