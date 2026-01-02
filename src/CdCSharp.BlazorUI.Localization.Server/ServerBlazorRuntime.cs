using CdCSharp.BlazorUI.Localization.Abstractions;

namespace CdCSharp.BlazorUI.Localization.Server;

public sealed class ServerBlazorRuntime : IBlazorRuntime
{
    public bool IsWasm => false;
    public bool IsServer => true;
}
