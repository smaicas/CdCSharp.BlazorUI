using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Loading;

[Trait("Component Snapshots", "BUILoadingIndicator")]
public class BUILoadingIndicatorSnapshotTests
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
                Name = "Spinner_Default",
                Html = ctx.Render<BUILoadingIndicator>().GetNormalizedMarkup()
            },
            new
            {
                Name = "Dots",
                Html = ctx.Render<BUILoadingIndicator>(p => p
                    .Add(c => c.Variant, BUILoadingIndicatorVariant.Dots)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Bars",
                Html = ctx.Render<BUILoadingIndicator>(p => p
                    .Add(c => c.Variant, BUILoadingIndicatorVariant.Bars)).GetNormalizedMarkup()
            },
            new
            {
                Name = "LinearIndeterminate",
                Html = ctx.Render<BUILoadingIndicator>(p => p
                    .Add(c => c.Variant, BUILoadingIndicatorVariant.LinearIndeterminate)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Spinner_Large_Custom_Label",
                Html = ctx.Render<BUILoadingIndicator>(p => p
                    .Add(c => c.Size, SizeEnum.Large)
                    .Add(c => c.AriaLabel, "Uploading file")).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
