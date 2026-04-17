using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Svg;

[Trait("Component Snapshots", "BUISvgIcon")]
public class BUISvgIconSnapshotTests
{
    private const string TriangleIcon = "<path d=\"M12 2L2 22h20L12 2z\"/>";
    private const string CircleIcon = "<circle cx=\"12\" cy=\"12\" r=\"10\"/>";
    private const string StarIcon = "<path d=\"M12 2l3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1z\"/>";

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Default_Triangle",
                Html = ctx.Render<BUISvgIcon>(p => p
                    .Add(c => c.Icon, TriangleIcon)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Circle_With_Title",
                Html = ctx.Render<BUISvgIcon>(p => p
                    .Add(c => c.Icon, CircleIcon)
                    .Add(c => c.Title, "Circle icon")).GetNormalizedMarkup()
            },
            new
            {
                Name = "Star_Large",
                Html = ctx.Render<BUISvgIcon>(p => p
                    .Add(c => c.Icon, StarIcon)
                    .Add(c => c.Size, SizeEnum.Large)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Custom_ViewBox",
                Html = ctx.Render<BUISvgIcon>(p => p
                    .Add(c => c.Icon, TriangleIcon)
                    .Add(c => c.ViewBox, "0 0 48 48")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
