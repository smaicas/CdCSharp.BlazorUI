using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

/// <summary>
/// Infrastructure services available to all tools.
/// </summary>
public sealed record InfrastructureServices
{
    public required IFileSystem FileSystem { get; init; }
    public required ITheonLogger Logger { get; init; }
    public required TheonOptions Options { get; init; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public string Serialize(object obj) => JsonSerializer.Serialize(obj, JsonOptions);
}

/// <summary>
/// Project-level knowledge and analysis.
/// </summary>
public sealed record ProjectKnowledge
{
    public required SharedProjectKnowledge Metadata { get; init; }
    public required IProjectContext Context { get; init; }
}

/// <summary>
/// Execution context including tracing and state.
/// </summary>
public sealed record ExecutionScope
{
    public required ContextState? State { get; init; }
    public required ContextConfiguration? Config { get; init; }
    public required int CloneDepth { get; init; }
}

/// <summary>
/// Orchestration capabilities for managing contexts.
/// </summary>
public sealed record OrchestrationCapabilities
{
    public required ContextRegistry Registry { get; init; }
    public required IContextFactory Factory { get; init; }
    public required ContextBudgetManager BudgetManager { get; init; }
    public required OrchestratorState? State { get; init; }
}