using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Snapshots", "BUIInputTextArea")]
public class BUIInputTextAreaSnapshotTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();

        var testCases = new[]
        {
            new { Name = "Default_Outlined", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Value", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.Value, "hello world")
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "AutoResize_Enabled", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.AutoResize, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Resize_None", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.Resize, TextAreaResize.None)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Resize_Both", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.Resize, TextAreaResize.Both)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Disabled", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Disabled")
                .Add(c => c.Disabled, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "Error_State", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Error")
                .Add(c => c.Error, true)
                .Add(c => c.ValueExpression, () => model.Value)) },

            new { Name = "With_Prefix_Suffix", Builder = (Action<ComponentParameterCollectionBuilder<BUIInputTextArea>>)(p => p
                .Add(c => c.Label, "Notes")
                .Add(c => c.PrefixText, "PRE")
                .Add(c => c.SuffixText, "SUF")
                .Add(c => c.ValueExpression, () => model.Value)) }
        };

        var results = testCases.Select(testCase =>
        {
            IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(testCase.Builder);
            return new
            {
                testCase.Name,
                Html = cut.GetNormalizedMarkup()
            };
        });

        await Verify(results).UseParameters(scenario.Name);
    }
}
