namespace CdCSharp.Theon.Orchestrator;

public interface IOrchestrator
{
    Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default);
}

public sealed record OrchestratorResponse(
    string Content,
    IReadOnlyList<GeneratedFile> OutputFiles,
    IReadOnlyList<string> ModifiedProjectFiles,
    float Confidence);

public sealed record GeneratedFile(string Name, string Content);

internal class Orchestrator : IOrchestrator
{
    public Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default) => throw new NotImplementedException();
}
