namespace CdCSharp.Theon.Core;

public sealed record LlmMessage(string Role, string Content)
{
    public static LlmMessage System(string content) => new("system", content);
    public static LlmMessage User(string content) => new("user", content);
    public static LlmMessage Assistant(string content) => new("assistant", content);
}

public sealed record LlmResponse(
    string Content,
    int PromptTokens,
    int CompletionTokens);

public sealed record ModelInfo(
    string Id,
    int ContextLength);

public enum ContextType
{
    Project,
    Assembly,
    File,
    Folder,
    MultiFile
}

#region Tool Invocations

public abstract record ToolInvocation;

public sealed record ExploreAssemblyTool(string Name) : ToolInvocation;
public sealed record ExploreFileTool(string Path) : ToolInvocation;
public sealed record ExploreFolderTool(string Path) : ToolInvocation;
public sealed record ExploreFilesTool(IReadOnlyList<string> Paths) : ToolInvocation;

public sealed record GenerateFileTool(string Name, string Language, string Content) : ToolInvocation;
public sealed record AppendFileTool(string Name, string Content) : ToolInvocation;
public sealed record OverwriteFileTool(string Name, string Language, string Content) : ToolInvocation;

public sealed record ModifyProjectFileTool(string Path, string Content) : ToolInvocation;

public sealed record ConfidenceTool(float Value) : ToolInvocation;
public sealed record NeedMoreContextTool(string Reason) : ToolInvocation;

#endregion

#region Orchestration Results

public sealed record OrchestratorResponse(
    string Content,
    IReadOnlyList<GeneratedFile> OutputFiles,
    IReadOnlyList<string> ModifiedProjectFiles,
    float Confidence);

public sealed record GeneratedFile(string Name, string Content);

public sealed record ScopeResult(
    string Content,
    IReadOnlyList<ToolInvocation> Tools,
    float Confidence,
    bool NeedsMoreContext,
    string? MoreContextReason);

#endregion