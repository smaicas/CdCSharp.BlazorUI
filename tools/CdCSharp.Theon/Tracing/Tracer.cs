using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace CdCSharp.Theon.Tracing;

public interface ITracer
{
    ITracerScope BeginOrchestration(string userInput);
    ITracerScope BeginContext(string contextName, string question, IReadOnlyList<string>? initialFiles = null);
}

public interface ITracerScope : IDisposable
{
    void RecordLlmRequest(ChatCompletionRequest request);
    void RecordLlmResponse(ChatCompletionResponse response, TimeSpan duration);
    void RecordToolExecution(ToolCall toolCall, string result, TimeSpan duration, bool isError = false);
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

    private const int PreviewLength = 200;

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

    private sealed class OrchestratorTracerScope : ITracerScope
    {
        private readonly ExecutionTrace _trace;
        private readonly Stopwatch _stopwatch;
        private readonly IFileSystem _fileSystem;
        private readonly JsonSerializerOptions _jsonOptions;
        private int _llmCallIndex;

        public OrchestratorTracerScope(string userInput, IFileSystem fileSystem, JsonSerializerOptions jsonOptions)
        {
            _trace = new ExecutionTrace { UserInput = userInput };
            _stopwatch = Stopwatch.StartNew();
            _fileSystem = fileSystem;
            _jsonOptions = jsonOptions;
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
                        ContentPreview = Truncate(m.Content, PreviewLength),
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
                ContentPreview = Truncate(choice.Message.Content, PreviewLength),
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
        }

        public void RecordToolExecution(ToolCall toolCall, string result, TimeSpan duration, bool isError = false)
        {
            ToolExecutionTrace execution = new()
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.Function.Name,
                Arguments = toolCall.Function.Arguments,
                DurationMs = (long)duration.TotalMilliseconds,
                ResultPreview = Truncate(result, PreviewLength),
                ResultLength = result.Length,
                IsError = isError
            };

            _trace.Orchestrator.ToolExecutions.Add(execution);
        }

        public void RecordFileLoaded(string path, int sizeBytes, int estimatedTokens)
        {
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

            string json = JsonSerializer.Serialize(_trace, _jsonOptions);
            string jsonFilename = $"trace_{_trace.Id}.json";
            _fileSystem.WriteOutputFileAsync("traces", jsonFilename, json).GetAwaiter().GetResult();

            string html = TraceHtmlGenerator.Generate(_trace);
            string htmlFilename = $"trace_{_trace.Id}.html";
            _fileSystem.WriteOutputFileAsync("traces", htmlFilename, html).GetAwaiter().GetResult();
        }

        private static string Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxLength) return text;
            return text[..maxLength] + "...";
        }
    }

    private sealed class ContextTracerScope : ITracerScope
    {
        private readonly ContextTrace _trace;
        private readonly Stopwatch _stopwatch;
        private int _llmCallIndex;

        public ContextTracerScope(string contextName, string question, IReadOnlyList<string>? initialFiles)
        {
            _trace = new ContextTrace
            {
                ContextName = contextName,
                Question = question,
                InitialFiles = initialFiles?.ToList() ?? []
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
                        ContentPreview = Truncate(m.Content, 200),
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
                ContentPreview = Truncate(choice.Message.Content, 200),
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
                ResultPreview = Truncate(result, 200),
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

        private static string Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxLength) return text;
            return text[..maxLength] + "...";
        }
    }
}