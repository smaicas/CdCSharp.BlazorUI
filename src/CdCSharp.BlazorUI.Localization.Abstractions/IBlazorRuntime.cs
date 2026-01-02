namespace CdCSharp.BlazorUI.Localization.Abstractions;

public interface IBlazorRuntime
{
    bool IsWasm { get; }
    bool IsServer { get; }
}
