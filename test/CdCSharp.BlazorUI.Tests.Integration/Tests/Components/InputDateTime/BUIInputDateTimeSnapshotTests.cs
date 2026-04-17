using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Snapshots", "BUIInputDateTime")]
public class BUIInputDateTimeSnapshotTests
{
    private class Model
    {
        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public DateTime? DateTime { get; set; }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();

        var testCases = new[]
        {
            new { Name = "Default_Empty_DateOnly", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.ValueExpression, () => model.Date)) },

            new { Name = "With_DateOnly_Value", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.ValueExpression, () => model.Date)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.Disabled, true)
                .Add(c => c.ValueExpression, () => model.Date)) },

            new { Name = "ReadOnly", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.ReadOnly, true)
                .Add(c => c.Value, new DateOnly(2024, 6, 15))
                .Add(c => c.ValueExpression, () => model.Date)) },

            new { Name = "Error_State", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.Error, true)
                .Add(c => c.ValueExpression, () => model.Date)) },

            new { Name = "With_Helper_Text", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputDateTime<DateOnly?>>>)(p => p
                .Add(c => c.Label, "Date")
                .Add(c => c.HelperText, "Select a date")
                .Add(c => c.ValueExpression, () => model.Date)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUIInputDateTime<DateOnly?>> cut =
                ctx.Render<BUIInputDateTime<DateOnly?>>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
