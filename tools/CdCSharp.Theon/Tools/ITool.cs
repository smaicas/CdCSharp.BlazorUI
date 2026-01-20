using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools;

/// <summary>
/// Represents a tool that can be executed by the LLM.
/// </summary>
public interface ITool<TResult>
{
    string ToolName { get; }
    bool RequiresConfirmation { get; }
    bool IsReadOnly { get; }
}

/// <summary>
/// Handles the execution of a specific tool.
/// </summary>
public interface IToolHandler<TTool, TResult>
    where TTool : ITool<TResult>
{
    Task<Result<TResult>> HandleAsync(TTool tool, ToolContext context, CancellationToken ct);
}

/// <summary>
/// Unified context for tool execution.
/// Contains all services and state needed by any tool.
/// </summary>
public sealed record ToolContext
{
    public required InfrastructureServices Infrastructure { get; init; }
    public required ProjectKnowledge Knowledge { get; init; }
    public required ExecutionScope Execution { get; init; }
    public OrchestrationCapabilities? Orchestration { get; init; }
}