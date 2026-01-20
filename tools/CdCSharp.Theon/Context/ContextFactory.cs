using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tracing;
using Microsoft.Extensions.Options;

namespace CdCSharp.Theon.Context;

public interface IContextFactory
{
    // Métodos públicos - para Orchestrator/usuario
    IContext Create(ContextConfiguration config);
    IContext CreateDynamic(string name, string purpose, bool stateful = false);
    IContext GetPredefined(PredefinedContext context);

    // Métodos internos - para contextos que delegan/clonan
    IContextScope CreateSibling(ContextConfiguration baseConfig, string purpose, int cloneDepth);
    IContextScope CreateDelegate(string targetContextType, string purpose);
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
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly PromptFormatter _promptFormatter;
    private readonly ContextBudgetManager _budgetManager;
    private readonly TheonOptions _options;
    private readonly Dictionary<string, ContextConfiguration> _predefinedConfigs;

    public ContextFactory(
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        PromptFormatter promptFormatter,
        ContextBudgetManager budgetManager,
        IOptions<TheonOptions> options)
    {
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _promptFormatter = promptFormatter;
        _budgetManager = budgetManager;
        _options = options.Value;

        _predefinedConfigs = BuildPredefinedConfigs();
    }

    private Dictionary<string, ContextConfiguration> BuildPredefinedConfigs()
    {
        string model = _options.Llm.Model;

        return new()
        {
            ["CodeExplorer"] = new ContextConfiguration
            {
                Name = "CodeExplorer",
                Model = model,
                ContextType = "CodeExplorer",
                Speciality = "Code implementation, patterns, algorithms",
                SystemPrompt = """
                    You are a C# code analysis expert.

                    ## Capabilities
                    - Explain implementation details
                    - Identify design patterns
                    - Trace data/control flow
                    - Detect code smells
                    - Analyze async/await and LINQ

                    ## Workflow
                    1. Check File Index for available files
                    2. Use read_file to load files (consumes budget)
                    3. Use peek_file for already-loaded files (free)
                    4. Use create_sub_context if budget exceeded
                    5. Provide analysis

                    ## Rules
                    - ALWAYS read/peek files before answering
                    - Use EXACT paths from File Index
                    - Check Active Contexts for already-loaded files
                    """,
                IsStateful = true,
                MaxTokenBudget = 16000,
                CanReadFiles = true,
                CanSearchFiles = true,
                CanDelegateToContexts = true,
                CanSpawnClones = true,
                MaxDelegationDepth = 2,
                MaxCloneDepth = 10,
                MaxClonesPerType = 50
            },

            ["ArchitectureAnalyzer"] = new ContextConfiguration
            {
                Name = "ArchitectureAnalyzer",
                Model = model,
                ContextType = "ArchitectureAnalyzer",
                Speciality = "Project structure, layers, architectural patterns",
                SystemPrompt = """
                    You are a software architecture expert for .NET solutions.

                    ## Capabilities
                    - Identify architectural styles
                    - Evaluate layer separation
                    - Assess dependency flow
                    - Detect violations
                    - Recommend improvements

                    ## Workflow
                    1. Review File Index for structure
                    2. Load key files (Program.cs, DI config)
                    3. Use peek_file for context-loaded files
                    4. Use create_sub_context for detailed analysis
                    5. Analyze relationships

                    ## Rules
                    - Use EXACT paths
                    - Start with entry points
                    - Look for interfaces and implementations
                    """,
                IsStateful = false,
                MaxTokenBudget = 20000,
                CanReadFiles = true,
                CanSearchFiles = true,
                CanDelegateToContexts = true,
                CanSpawnClones = true,
                MaxDelegationDepth = 2,
                MaxCloneDepth = 10,
                MaxClonesPerType = 50
            },

            ["DependencyAnalyzer"] = new ContextConfiguration
            {
                Name = "DependencyAnalyzer",
                Model = model,
                ContextType = "DependencyAnalyzer",
                Speciality = "Dependencies, DI configuration, coupling",
                SystemPrompt = """
                    You are a dependency analysis expert for C#/.NET.

                    ## Capabilities
                    - Trace type dependencies
                    - Identify circular dependencies
                    - Map interface implementations
                    - Analyze DI containers
                    - Evaluate coupling

                    ## Workflow
                    1. Use File Index to locate files
                    2. Load DI configuration files
                    3. Peek files from other contexts
                    4. Trace through constructors
                    5. Use create_sub_context for large graphs

                    ## Rules
                    - Use EXACT paths
                    - Focus on interfaces and DI
                    - Look for ServiceCollection extensions
                    """,
                IsStateful = false,
                MaxTokenBudget = 18000,
                CanReadFiles = true,
                CanSearchFiles = true,
                CanDelegateToContexts = true,
                CanSpawnClones = true,
                MaxDelegationDepth = 2,
                MaxCloneDepth = 10,
                MaxClonesPerType = 50
            }
        };
    }

    public IContext Create(ContextConfiguration config)
    {
        _logger.Debug($"Creating context: {config.Name} (type: {config.ContextType})");
        return new Context(
            config,
            _aiClient,
            _projectContext,
            _fileSystem,
            _logger,
            _tracer,
            this,
            _sharedKnowledge,
            _registry,
            _promptFormatter,
            _budgetManager,
            _options);
    }

    public IContext CreateDynamic(string name, string purpose, bool stateful = false)
    {
        ContextConfiguration config = new()
        {
            Name = name,
            Model = _options.Llm.Model,
            ContextType = "Dynamic",
            Speciality = purpose,
            SystemPrompt = $"""
                You are a specialized assistant for: {purpose}

                ## Workflow
                1. Check File Index
                2. Use read_file to load files
                3. Use peek_file for context-loaded files
                4. Provide analysis

                ## Rules
                - Use EXACT paths from File Index
                - Check Active Contexts
                """,
            IsStateful = stateful,
            MaxTokenBudget = 8000,
            CanReadFiles = true,
            CanSearchFiles = true,
            CanDelegateToContexts = true,
            CanSpawnClones = true
        };

        return Create(config);
    }

    public IContext GetPredefined(PredefinedContext context)
    {
        string contextType = context.ToString();
        if (!_predefinedConfigs.TryGetValue(contextType, out ContextConfiguration? config))
            throw new ArgumentException($"Unknown predefined context: {context}");

        return Create(config);
    }

    public IContextScope CreateSibling(ContextConfiguration baseConfig, string purpose, int cloneDepth)
    {
        string cloneName = _registry.GenerateCloneName(baseConfig.ContextType);

        ContextConfiguration cloneConfig = baseConfig with
        {
            Name = cloneName,
            IsStateful = true
        };

        return new ContextScope(
            cloneConfig,
            _aiClient,
            _projectContext,
            _fileSystem,
            _logger,
            _tracer,
            this,
            _sharedKnowledge,
            _registry,
            _promptFormatter,
            _budgetManager,
            _options,
            cloneDepth);
    }

    public IContextScope CreateDelegate(string targetContextType, string purpose)
    {
        if (!_predefinedConfigs.TryGetValue(targetContextType, out ContextConfiguration? config))
        {
            throw new ArgumentException($"Unknown context type: {targetContextType}");
        }

        return new ContextScope(
            config,
            _aiClient,
            _projectContext,
            _fileSystem,
            _logger,
            _tracer,
            this,
            _sharedKnowledge,
            _registry,
            _promptFormatter,
            _budgetManager,
            _options,
            cloneDepth: 0);
    }
}