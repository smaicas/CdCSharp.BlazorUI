using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace CdCSharp.Theon.Tracing;

internal sealed class OrchestratorTracerScope : ITracerScope
{
    private readonly ExecutionTrace _trace;
    private readonly Stopwatch _stopwatch;
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly PartialTracer _partialTracer;
    private int _llmCallIndex;

    public string TraceId => _trace.Id;

    public OrchestratorTracerScope(string userInput, IFileSystem fileSystem, JsonSerializerOptions jsonOptions)
    {
        _trace = new ExecutionTrace { UserInput = userInput };
        _stopwatch = Stopwatch.StartNew();
        _fileSystem = fileSystem;
        _jsonOptions = jsonOptions;
        _partialTracer = new PartialTracer(fileSystem);
        _partialTracer.SetUserInput(userInput);
    }

    public void RecordLlmRequest(ChatCompletionRequest request)
    {
        LlmCallTrace call = new()
        {
            Index = _llmCallIndex++,
            Request = new LlmRequestTrace
            {
                Model = request.Model,
                MessageCount = request.Messages.Count,
                Messages = request.Messages.Select(m => new MessageTrace
                {
                    Role = m.Role,
                    Content = m.Content ?? "",
                    ContentLength = m.Content?.Length ?? 0,
                    ToolCallId = m.ToolCallId,
                    HasToolCalls = m.ToolCalls?.Count > 0
                }).ToList(),
                Tools = request.Tools?.Select(t => t.Function.Name).ToList() ?? [],
                HasResponseFormat = request.ResponseFormat != null
            }
        };

        _trace.Orchestrator.LlmCalls.Add(call);
    }

    public void RecordLlmResponse(ChatCompletionResponse response, TimeSpan duration)
    {
        if (_trace.Orchestrator.LlmCalls.Count == 0) return;

        LlmCallTrace currentCall = _trace.Orchestrator.LlmCalls[^1];
        Choice choice = response.Choices[0];

        currentCall.DurationMs = (long)duration.TotalMilliseconds;
        currentCall.Response = new LlmResponseTrace
        {
            FinishReason = choice.FinishReason ?? "unknown",
            Content = choice.Message.Content,
            ContentLength = choice.Message.Content?.Length ?? 0,
            ToolCalls = choice.Message.ToolCalls?.Select(tc => new ToolCallTrace
            {
                Id = tc.Id,
                Name = tc.Function.Name,
                Arguments = tc.Function.Arguments
            }).ToList(),
            Tokens = response.Usage != null ? new TokenUsageTrace
            {
                Prompt = response.Usage.PromptTokens,
                Completion = response.Usage.CompletionTokens,
                Total = response.Usage.TotalTokens
            } : null
        };

        if (response.Usage != null)
        {
            _trace.TotalTokens += response.Usage.TotalTokens;
        }
        _trace.TotalLlmCalls++;

        // Write partial trace
        _partialTracer.RecordLlmCall(
            // Reconstruct request from trace
            new ChatCompletionRequest
            {
                Model = currentCall.Request.Model,
                Messages = currentCall.Request.Messages.Select(m => new Message
                {
                    Role = m.Role,
                    Content = m.Content
                }).ToList()
            },
            response,
            duration);
    }

    public void RecordToolExecution(ToolCall toolCall, string result, TimeSpan duration, bool isError = false)
    {
        ToolExecutionTrace execution = new()
        {
            ToolCallId = toolCall.Id,
            ToolName = toolCall.Function.Name,
            Arguments = toolCall.Function.Arguments,
            DurationMs = (long)duration.TotalMilliseconds,
            Content = result,
            ResultLength = result.Length,
            IsError = isError
        };

        _trace.Orchestrator.ToolExecutions.Add(execution);

        // Write partial trace
        _partialTracer.RecordToolExecution(
            toolCall.Id,
            toolCall.Function.Name,
            toolCall.Function.Arguments,
            result,
            duration,
            isError);
    }

    public void RecordFileLoaded(string path, int sizeBytes, int estimatedTokens)
    {
        // Not implemented for orchestrator
    }

    public void RecordContextQuery(ContextTrace contextTrace)
    {
        ToolExecutionTrace? lastTool = _trace.Orchestrator.ToolExecutions.LastOrDefault();
        if (lastTool != null && lastTool.ToolName == "query_context")
        {
            lastTool.ContextTrace = contextTrace;
            _trace.TotalTokens += contextTrace.TotalTokens;
            _trace.TotalLlmCalls += contextTrace.LlmCalls.Count;
        }
    }

    public void SetResult(ExecutionResult result)
    {
        _trace.Result = result;
    }

    public ExecutionTrace GetTrace()
    {
        _stopwatch.Stop();
        _trace.DurationMs = _stopwatch.ElapsedMilliseconds;
        return _trace;
    }

    public ContextTrace GetContextTrace() => throw new NotSupportedException();

    public void Dispose()
    {
        _stopwatch.Stop();
        _trace.DurationMs = _stopwatch.ElapsedMilliseconds;

        // Dispose partial tracer first (writes final state)
        _partialTracer.Dispose();

        // Then write full trace to responses/traces
        string json = JsonSerializer.Serialize(_trace, _jsonOptions);
        string jsonFilename = $"trace_{_trace.Id}.json";
        _fileSystem.WriteOutputFileAsync("traces", jsonFilename, json).GetAwaiter().GetResult();

        string html = TraceHtmlGenerator.Generate(_trace);
        string htmlFilename = $"trace_{_trace.Id}.html";
        _fileSystem.WriteOutputFileAsync("traces", htmlFilename, html).GetAwaiter().GetResult();
    }
}