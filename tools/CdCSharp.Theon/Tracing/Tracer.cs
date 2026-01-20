using CdCSharp.Theon.Infrastructure;
using System.Text.Json;

namespace CdCSharp.Theon.Tracing;

public interface ITracer
{
    ITracerScope BeginOrchestration(string userInput);
    ITracerScope BeginContext(string contextName, string question, IReadOnlyList<string>? initialFiles = null);
}

public interface ITracerScope : IDisposable
{
    void RecordLlmRequest(AI.ChatCompletionRequest request);
    void RecordLlmResponse(AI.ChatCompletionResponse response, TimeSpan duration);
    void RecordToolExecution(AI.ToolCall toolCall, string result, TimeSpan duration, bool isError = false);
    void RecordFileLoaded(string path, int sizeBytes, int estimatedTokens);
    void RecordContextQuery(ContextTrace contextTrace);
    void SetResult(ExecutionResult result);

    ExecutionTrace GetTrace();
    ContextTrace GetContextTrace();
}

public sealed class Tracer : ITracer
{
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _jsonOptions;

    public Tracer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public ITracerScope BeginOrchestration(string userInput)
    {
        return new OrchestratorTracerScope(userInput, _fileSystem, _jsonOptions);
    }

    public ITracerScope BeginContext(string contextName, string question, IReadOnlyList<string>? initialFiles = null)
    {
        return new ContextTracerScope(contextName, question, initialFiles);
    }
}