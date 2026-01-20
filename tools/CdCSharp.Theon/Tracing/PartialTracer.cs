using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tracing;
using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// Writes partial traces incrementally to disk.
/// Allows inspection even if execution is cancelled or fails.
/// </summary>
public sealed class PartialTracer : IDisposable
{
    private readonly string _traceId;
    private readonly string _tracePath;
    private readonly IFileSystem _fileSystem;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();

    // Accumulators for partial data
    private readonly List<LlmCallTrace> _llmCalls = [];
    private readonly List<ToolExecutionTrace> _toolExecutions = [];
    private readonly Stopwatch _stopwatch;
    private string _userInput = string.Empty;
    private bool _disposed;

    public string TraceId => _traceId;

    public PartialTracer(IFileSystem fileSystem)
    {
        _traceId = Guid.NewGuid().ToString("N")[..12];
        _tracePath = Path.Combine(".theon", "traces", $"partial_{_traceId}.json");
        _fileSystem = fileSystem;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        _stopwatch = Stopwatch.StartNew();

        // Write initial marker
        WritePartial();
    }

    public void SetUserInput(string input)
    {
        lock (_lock)
        {
            _userInput = input;
            WritePartial();
        }
    }

    public void RecordLlmCall(ChatCompletionRequest request, ChatCompletionResponse? response, TimeSpan duration)
    {
        lock (_lock)
        {
            LlmCallTrace call = new()
            {
                Index = _llmCalls.Count,
                Timestamp = DateTime.UtcNow,
                DurationMs = (long)duration.TotalMilliseconds,
                Request = new LlmRequestTrace
                {
                    Model = request.Model,
                    MessageCount = request.Messages.Count,
                    Messages = request.Messages.Select(m => new MessageTrace
                    {
                        Role = m.Role,
                        Content = TruncateContent(m.Content),
                        ContentLength = m.Content?.Length ?? 0,
                        ToolCallId = m.ToolCallId,
                        HasToolCalls = m.ToolCalls?.Count > 0
                    }).ToList(),
                    Tools = request.Tools?.Select(t => t.Function.Name).ToList() ?? [],
                    HasResponseFormat = request.ResponseFormat != null
                },
                Response = response != null ? new LlmResponseTrace
                {
                    FinishReason = response.Choices[0].FinishReason ?? "unknown",
                    Content = TruncateContent(response.Choices[0].Message.Content),
                    ContentLength = response.Choices[0].Message.Content?.Length ?? 0,
                    ToolCalls = response.Choices[0].Message.ToolCalls?.Select(tc => new ToolCallTrace
                    {
                        Id = tc.Id,
                        Name = tc.Function.Name,
                        Arguments = TruncateContent(tc.Function.Arguments)
                    }).ToList(),
                    Tokens = response.Usage != null ? new TokenUsageTrace
                    {
                        Prompt = response.Usage.PromptTokens,
                        Completion = response.Usage.CompletionTokens,
                        Total = response.Usage.TotalTokens
                    } : null
                } : new LlmResponseTrace()
            };

            _llmCalls.Add(call);
            WritePartial();
        }
    }

    public void RecordToolExecution(string toolCallId, string toolName, string arguments, string result, TimeSpan duration, bool isError)
    {
        lock (_lock)
        {
            ToolExecutionTrace execution = new()
            {
                ToolCallId = toolCallId,
                ToolName = toolName,
                Arguments = TruncateContent(arguments),
                Timestamp = DateTime.UtcNow,
                DurationMs = (long)duration.TotalMilliseconds,
                Content = TruncateContent(result),
                ResultLength = result.Length,
                IsError = isError
            };

            _toolExecutions.Add(execution);
            WritePartial();
        }
    }

    private void WritePartial()
    {
        if (_disposed) return;

        try
        {
            var partial = new
            {
                trace_id = _traceId,
                status = "in_progress",
                timestamp = DateTime.UtcNow,
                duration_ms = _stopwatch.ElapsedMilliseconds,
                user_input = _userInput,
                llm_calls = _llmCalls.Count,
                tool_executions = _toolExecutions.Count,
                calls = _llmCalls,
                tools = _toolExecutions
            };

            string json = JsonSerializer.Serialize(partial, _jsonOptions);

            // Write to traces folder (not responses/traces)
            string tracesDir = Path.Combine(".theon", "traces");
            Directory.CreateDirectory(tracesDir);

            string fullPath = Path.Combine(tracesDir, $"partial_{_traceId}.json");
            File.WriteAllText(fullPath, json);
        }
        catch
        {
            // Silently fail - don't crash on trace write errors
        }
    }

    private static string? TruncateContent(string? content, int maxLength = 2000)
    {
        if (string.IsNullOrEmpty(content)) return content;
        if (content.Length <= maxLength) return content;
        return content[..maxLength] + "... [truncated]";
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;

            _stopwatch.Stop();

            try
            {
                var final = new
                {
                    trace_id = _traceId,
                    status = "completed",
                    timestamp = DateTime.UtcNow,
                    duration_ms = _stopwatch.ElapsedMilliseconds,
                    user_input = _userInput,
                    llm_calls = _llmCalls.Count,
                    tool_executions = _toolExecutions.Count,
                    calls = _llmCalls,
                    tools = _toolExecutions
                };

                string json = JsonSerializer.Serialize(final, _jsonOptions);

                string tracesDir = Path.Combine(".theon", "traces");
                string fullPath = Path.Combine(tracesDir, $"partial_{_traceId}.json");
                File.WriteAllText(fullPath, json);
            }
            catch
            {
                // Silent fail
            }
        }
    }
}
