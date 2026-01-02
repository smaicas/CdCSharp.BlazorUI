using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components;

[Trait("Component Snapshot", "UIButton")]
public class BUIButtonTests_Snapshot
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Default_button_renders_correctly(BlazorTestContextBase ctx)
    {
        BUIButtonVariant[] variants =
        [
            BUIButtonVariant.Default,
        ];

        var results = variants.Select(variant =>
        {
            IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(parameters => parameters
                .Add(p => p.Variant, variant)
                .Add(p => p.Text, $"{variant.Name} Button"));

            return new { Variant = variant.Name, Html = cut.Markup };
        });

        results.Should().AllSatisfy(r => r.Html.Should().Contain("bui-component"));

        await Verify(results).UseMethodName($"{nameof(BUIButton)}_{ctx.Scenario}");

        await ctx.DisposeAsync();
    }
}
