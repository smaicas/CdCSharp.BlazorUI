namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public interface ITestHostingModel
{
    bool IsServer { get; }
    bool IsWasm { get; }
}

public sealed class ServerHostingModel : ITestHostingModel
{
    public bool IsServer => true;
    public bool IsWasm => false;
}

public sealed class WasmHostingModel : ITestHostingModel
{
    public bool IsServer => false;
    public bool IsWasm => true;
}
