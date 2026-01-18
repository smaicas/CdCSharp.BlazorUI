namespace CdCSharp.Theon.Core;

public sealed record LlmMessage
{
    public string Role { get; init; }
    public string Content { get; init; }
    public IReadOnlyList<LlmToolCall>? ToolCalls { get; init; }
    public string? ToolCallId { get; init; }

    public LlmMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }

    private LlmMessage(string role, string content, IReadOnlyList<LlmToolCall>? toolCalls, string? toolCallId)
    {
        Role = role;
        Content = content;
        ToolCalls = toolCalls;
        ToolCallId = toolCallId;
    }

    public static LlmMessage System(string content) => new("system", content);
    public static LlmMessage User(string content) => new("user", content);
    public static LlmMessage Assistant(string content) => new("assistant", content);
    public static LlmMessage AssistantWithToolCalls(IReadOnlyList<LlmToolCall> toolCalls) =>
        new("assistant", string.Empty, toolCalls, null);
    public static LlmMessage ToolResult(string toolCallId, string content) =>
        new("tool", content, null, toolCallId);
}

public sealed record LlmToolCall(string Id, string Name, string Arguments);

public sealed record LlmResponse(
    string Content,
    IReadOnlyList<LlmToolCall>? ToolCalls,
    int PromptTokens,
    int CompletionTokens)
{
    public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;
}

public sealed record ModelInfo(string Id, int ContextLength);

public enum ContextType
{
    Project,
    Assembly,
    File,
    Folder,
    MultiFile
}

public sealed record OrchestratorResponse(
    string Content,
    IReadOnlyList<GeneratedFile> OutputFiles,
    IReadOnlyList<string> ModifiedProjectFiles,
    float Confidence);

public sealed record GeneratedFile(string Name, string Content);