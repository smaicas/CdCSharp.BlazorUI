using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator;
using CdCSharp.Theon.Tracing;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

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

public sealed record ProjectKnowledge
{
    public required SharedProjectKnowledge Metadata { get; init; }
    public required IProjectContext Context { get; init; }
}

public sealed record ExecutionScope
{
    public required ITracerScope? Tracer { get; init; }
    public required ContextState? State { get; init; }
    public required ContextConfiguration? Config { get; init; }
    public required int CloneDepth { get; init; }
}
public sealed record OrchestrationCapabilities
{
    public required ContextRegistry Registry { get; init; }
    public required IContextFactory Factory { get; init; }
    public required ContextBudgetManager BudgetManager { get; init; }
    public required OrchestratorState? State { get; init; }
}
public sealed record QueryContext
{
    public required InfrastructureServices Infrastructure { get; init; }
    public required ProjectKnowledge Knowledge { get; init; }
    public required ExecutionScope Execution { get; init; }
    public OrchestrationCapabilities? Orchestration { get; init; }
}
public sealed record CommandContext
{
    public required InfrastructureServices Infrastructure { get; init; }
    public required ProjectKnowledge Knowledge { get; init; }
    public required ExecutionScope Execution { get; init; }
    public required OrchestrationCapabilities? Orchestration { get; init; }
}