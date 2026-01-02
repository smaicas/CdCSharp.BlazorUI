using CdCSharp.BlazorUI.Localization.Abstractions;

namespace CdCSharp.BlazorUI.Services;

public sealed class MissingBlazorRuntime : IBlazorRuntime
{
    public bool IsWasm => throw new InvalidOperationException(
        "BUICultureSelector requires a registered IBlazorRuntime. " +
        "Use AddBlazorUILocalizationServer() or AddBlazorUILocalizationWasm() depending on hosting.");
    public bool IsServer => throw new InvalidOperationException(
        "BUICultureSelector requires a registered IBlazorRuntime. " +
        "Use AddBlazorUILocalizationServer() or AddBlazorUILocalizationWasm() depending on hosting.");
}
