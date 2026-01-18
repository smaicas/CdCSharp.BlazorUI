using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tracing;

namespace CdCSharp.Theon.Context;

public interface IContextFactory
{
    IContext Create(ContextConfiguration config);
    IContext CreateDynamic(string name, string purpose, bool stateful = false);
    IContext GetPredefined(PredefinedContext context);
}

public enum PredefinedContext
{
    CodeExplorer,
    ArchitectureAnalyzer,
    DependencyAnalyzer
}

public sealed class ContextFactory : IContextFactory
{
    private readonly IAIClient _aiClient;
    private readonly IProjectContext _projectContext;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly ITracer _tracer;

    private static readonly Dictionary<PredefinedContext, ContextConfiguration> PredefinedConfigs = new()
    {
        [PredefinedContext.CodeExplorer] = new ContextConfiguration
        {
            Name = "CodeExplorer",
            SystemPrompt = """
                You are a code exploration assistant. Your role is to help understand 
                specific pieces of code, explain their functionality, and trace 
                dependencies between components.
                
                When analyzing code:
                - Explain what the code does in clear terms
                - Identify key patterns and design decisions
                - Point out dependencies and relationships with other components
                """,
            IsStateful = true,
            MaxTokenBudget = 12000
        },

        [PredefinedContext.ArchitectureAnalyzer] = new ContextConfiguration
        {
            Name = "ArchitectureAnalyzer",
            SystemPrompt = """
                You are an architecture analysis assistant. Your role is to analyze 
                the overall structure of the project, identify architectural patterns, 
                and evaluate design decisions.
                
                When analyzing architecture:
                - Identify the architectural style (layered, clean architecture, etc.)
                - Map dependencies between assemblies
                - Evaluate separation of concerns
                - Identify potential improvements
                """,
            IsStateful = false,
            MaxTokenBudget = 16000
        },

        [PredefinedContext.DependencyAnalyzer] = new ContextConfiguration
        {
            Name = "DependencyAnalyzer",
            SystemPrompt = """
                You are a dependency analysis assistant. Your role is to trace and 
                explain dependencies between types, namespaces, and assemblies.
                
                When analyzing dependencies:
                - Trace the dependency chain for specific types
                - Identify circular dependencies
                - Map interface implementations
                - Suggest dependency injection improvements
                """,
            IsStateful = false,
            MaxTokenBudget = 10000
        }
    };

    public ContextFactory(
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer)
    {
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
    }

    public IContext Create(ContextConfiguration config)
    {
        _logger.Debug($"Creating context: {config.Name} (stateful: {config.IsStateful})");

        return new Context(config, _aiClient, _projectContext, _fileSystem, _logger, _tracer);
    }

    public IContext CreateDynamic(string name, string purpose, bool stateful = false)
    {
        ContextConfiguration config = new()
        {
            Name = name,
            SystemPrompt = $"""
                You are a specialized assistant for: {purpose}
                
                Focus on answering questions related to this specific purpose.
                Use the available tools to explore the codebase as needed.
                """,
            IsStateful = stateful,
            MaxTokenBudget = 8000
        };

        return Create(config);
    }

    public IContext GetPredefined(PredefinedContext context)
    {
        if (!PredefinedConfigs.TryGetValue(context, out ContextConfiguration? config))
        {
            throw new ArgumentException($"Unknown predefined context: {context}");
        }

        return Create(config);
    }
}