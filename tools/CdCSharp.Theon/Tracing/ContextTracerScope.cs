using CdCSharp.Theon.AI;
using System.Diagnostics;

namespace CdCSharp.Theon.Tracing;

internal sealed class ContextTracerScope : ITracerScope
{
    private readonly ContextTrace _trace;
    private readonly Stopwatch _stopwatch;
    private int _llmCallIndex;

    public string TraceId => throw new NotImplementedException();

    public ContextTracerScope(string contextName, string question, IReadOnlyList<string>? initialFiles)
    {
        _trace = new ContextTrace
        {
            ContextName = contextName,
            Question = question,
            InitialFiles = initialFiles?.ToList() ?? [],
            DelegationDepth = 0
        };
        _stopwatch = Stopwatch.StartNew();
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

        _trace.LlmCalls.Add(call);
    }

    public void RecordLlmResponse(ChatCompletionResponse response, TimeSpan duration)
    {
        if (_trace.LlmCalls.Count == 0) return;

        LlmCallTrace currentCall = _trace.LlmCalls[^1];
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
    }

    public void RecordToolExecution(ToolCall toolCall, string result, TimeSpan duration, bool isError = false)
    {
        _trace.ToolExecutions.Add(new ToolExecutionTrace
        {
            ToolCallId = toolCall.Id,
            ToolName = toolCall.Function.Name,
            Arguments = toolCall.Function.Arguments,
            DurationMs = (long)duration.TotalMilliseconds,
            Content = result,
            ResultLength = result.Length,
            IsError = isError
        });
    }

    public void RecordFileLoaded(string path, int sizeBytes, int estimatedTokens)
    {
        _trace.FilesLoaded.Add(new FileLoadTrace
        {
            Path = path,
            SizeBytes = sizeBytes,
            EstimatedTokens = estimatedTokens
        });
    }

    public void RecordContextQuery(ContextTrace contextTrace)
    {
        _trace.DelegatedContexts.Add(contextTrace);
        _trace.TotalTokens += contextTrace.TotalTokens;
    }

    public void SetResult(ExecutionResult result)
    {
    }

    public ExecutionTrace GetTrace() => throw new NotSupportedException();

    public ContextTrace GetContextTrace()
    {
        _stopwatch.Stop();
        return _trace;
    }

    public void Dispose()
    {
        _stopwatch.Stop();
    }
}