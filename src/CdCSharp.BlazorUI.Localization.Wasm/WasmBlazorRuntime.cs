using CdCSharp.BlazorUI.Localization.Abstractions;

namespace CdCSharp.BlazorUI.Localization.Wasm;

public sealed class WasmBlazorRuntime : IBlazorRuntime
{
    public bool IsWasm => true;
    public bool IsServer => false;
}
