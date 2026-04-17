using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.SyntaxHighlight;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Snapshots", "BUICodeBlock")]
public class BUICodeBlockSnapshotTests
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
                Name = "CSharp_Default",
                Html = ctx.Render<BUICodeBlock>(p => p
                    .Add(c => c.Code, "var x = 1;")
                    .Add(c => c.Language, SyntaxHighlightLanguage.CSharp)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Json_With_Custom_Title",
                Html = ctx.Render<BUICodeBlock>(p => p
                    .Add(c => c.Code, "{ \"key\": \"value\" }")
                    .Add(c => c.Language, SyntaxHighlightLanguage.Json)
                    .Add(c => c.Title, "Config")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BUICodeBlock>(p => p
                    .Add(c => c.Code, "var x = 1;")
                    .Add(c => c.Size, SizeEnum.Large)).GetNormalizedMarkup()
            },
            new
            {
                Name = "TypeScript",
                Html = ctx.Render<BUICodeBlock>(p => p
                    .Add(c => c.Code, "const x: number = 1;")
                    .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
