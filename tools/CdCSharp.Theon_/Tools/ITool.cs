// Tools/ITool.cs
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    ToolCategory Category { get; }

    ToolPromptSpec GetPromptSpec();
    ToolDefinition GetDefinition();

    Task<ToolExecutionResult> ExecuteAsync(
        JsonElement parameters,
        ToolExecutionContext context,
        CancellationToken ct = default);
}

public enum ToolCategory
{
    Exploration,
    Output,
    Modification
}

public sealed record ToolPromptSpec(
    string Syntax,
    string? ClosingTag,
    string Example,
    string[] ParameterDescriptions);

public sealed record ToolDefinition(
    string Name,
    string Description,
    JsonElement ParametersSchema);

public sealed record ToolExecutionResult(
    bool Success,
    string? ContextToAdd,
    string? ErrorMessage)
{
    public static ToolExecutionResult Ok(string context) => new(true, context, null);
    public static ToolExecutionResult Fail(string error) => new(false, null, error);
}

public sealed record ToolExecutionContext(
    string? CurrentQuery,
    IServiceProvider Services);