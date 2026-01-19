using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Tracing;

public sealed class ExecutionTrace
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..12];

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("user_input")]
    public string UserInput { get; init; } = string.Empty;

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("total_llm_calls")]
    public int TotalLlmCalls { get; set; }

    [JsonPropertyName("orchestrator")]
    public OrchestratorTrace Orchestrator { get; init; } = new();

    [JsonPropertyName("result")]
    public ExecutionResult Result { get; set; } = new();
}

public sealed class OrchestratorTrace
{
    [JsonPropertyName("llm_calls")]
    public List<LlmCallTrace> LlmCalls { get; init; } = [];

    [JsonPropertyName("tool_executions")]
    public List<ToolExecutionTrace> ToolExecutions { get; init; } = [];
}

public sealed class LlmCallTrace
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }

    [JsonPropertyName("request")]
    public LlmRequestTrace Request { get; init; } = new();

    [JsonPropertyName("response")]
    public LlmResponseTrace Response { get; set; } = new();
}

public sealed class LlmRequestTrace
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("message_count")]
    public int MessageCount { get; init; }

    [JsonPropertyName("messages")]
    public List<MessageTrace> Messages { get; init; } = [];

    [JsonPropertyName("tools")]
    public List<string> Tools { get; init; } = [];

    [JsonPropertyName("has_response_format")]
    public bool HasResponseFormat { get; init; }
}

public sealed class MessageTrace
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("content_length")]
    public int ContentLength { get; init; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; init; }

    [JsonPropertyName("has_tool_calls")]
    public bool HasToolCalls { get; init; }
}

public sealed class LlmResponseTrace
{
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("content_length")]
    public int ContentLength { get; init; }

    [JsonPropertyName("tool_calls")]
    public List<ToolCallTrace>? ToolCalls { get; init; }

    [JsonPropertyName("tokens")]
    public TokenUsageTrace? Tokens { get; init; }
}

public sealed class ToolCallTrace
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; init; } = string.Empty;
}

public sealed class TokenUsageTrace
{
    [JsonPropertyName("prompt")]
    public int Prompt { get; init; }

    [JsonPropertyName("completion")]
    public int Completion { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }
}

public sealed class ToolExecutionTrace
{
    [JsonPropertyName("tool_call_id")]
    public string ToolCallId { get; init; } = string.Empty;

    [JsonPropertyName("tool_name")]
    public string ToolName { get; init; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("duration_ms")]
    public long DurationMs { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("result_length")]
    public int ResultLength { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; }

    [JsonPropertyName("context_trace")]
    public ContextTrace? ContextTrace { get; set; }
}

public sealed class ContextTrace
{
    [JsonPropertyName("context_name")]
    public string ContextName { get; init; } = string.Empty;

    [JsonPropertyName("question")]
    public string Question { get; init; } = string.Empty;

    [JsonPropertyName("initial_files")]
    public List<string> InitialFiles { get; init; } = [];

    [JsonPropertyName("llm_calls")]
    public List<LlmCallTrace> LlmCalls { get; init; } = [];

    [JsonPropertyName("tool_executions")]
    public List<ToolExecutionTrace> ToolExecutions { get; init; } = [];

    [JsonPropertyName("files_loaded")]
    public List<FileLoadTrace> FilesLoaded { get; init; } = [];

    [JsonPropertyName("delegated_contexts")]
    public List<ContextTrace> DelegatedContexts { get; init; } = [];

    [JsonPropertyName("delegation_depth")]
    public int DelegationDepth { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public sealed class FileLoadTrace
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("size_bytes")]
    public int SizeBytes { get; init; }

    [JsonPropertyName("estimated_tokens")]
    public int EstimatedTokens { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = "file";
}

public sealed class ExecutionResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("message_preview")]
    public string MessagePreview { get; init; } = string.Empty;

    [JsonPropertyName("created_files")]
    public List<string> CreatedFiles { get; init; } = [];

    [JsonPropertyName("generated_outputs")]
    public List<string> GeneratedOutputs { get; init; } = [];

    [JsonPropertyName("proposed_changes")]
    public List<ProposedChangeTrace> ProposedChanges { get; init; } = [];

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

public sealed class ProposedChangeTrace
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("change_type")]
    public string ChangeType { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}