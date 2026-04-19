using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using ServerSelector = CdCSharp.BlazorUI.Components.Server.BUICultureSelector;
using ServerVariant = CdCSharp.BlazorUI.Components.Server.BUICultureSelectorVariant;
using WasmSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using WasmVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Snapshots", "BUICultureSelector")]
public class BUICultureSelectorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Dropdown_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Dropdown)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup()
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Dropdown)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup();

        await Verifier.Verify(markup).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Flags_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup()
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, true)).GetNormalizedMarkup();

        await Verifier.Verify(markup).UseParameters(scenario.Name);
    }
}
