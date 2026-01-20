using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tracing;
using Microsoft.Extensions.Options;

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
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly TheonOptions _options;

    private readonly Dictionary<PredefinedContext, ContextConfiguration> _predefinedConfigs;

    public ContextFactory(
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        IOptions<TheonOptions> options)
    {
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _options = options.Value;

        _predefinedConfigs = BuildPredefinedConfigs();
    }

    private Dictionary<PredefinedContext, ContextConfiguration> BuildPredefinedConfigs()
    {
        string model = _options.Llm.Model;

        return new()
        {
            [PredefinedContext.CodeExplorer] = new ContextConfiguration
            {
                Name = "CodeExplorer",
                Model = model,
                ContextType = "CodeExplorer",
                Speciality = "Code implementation, patterns, algorithms, and logic analysis",
                SystemPrompt = """
                    You are a C# code analysis expert. Your job is to examine source code and explain how it works.

                    ## Your Capabilities
                    - Explain implementation details, algorithms, and logic
                    - Identify design patterns (Repository, Factory, Strategy, DI, etc.)
                    - Trace data flow and control flow
                    - Detect code smells and potential issues
                    - Analyze async/await patterns, LINQ, and modern C# features

                    ## How to Work
                    1. Look at the File Index to see available files
                    2. Use read_file with EXACT paths to load files into your context
                    3. Use peek_file to temporarily view files (doesn't use your budget)
                    4. Use spawn_clone if you need to analyze more files than fit in your budget
                    5. Provide your analysis when you have enough information

                    ## Important Rules
                    - ALWAYS use read_file or peek_file before answering questions about code
                    - Use EXACT paths from the File Index
                    - Check "Active Contexts" to see what files other contexts have loaded
                    - Use peek_file for files already loaded by other contexts
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

            [PredefinedContext.ArchitectureAnalyzer] = new ContextConfiguration
            {
                Name = "ArchitectureAnalyzer",
                Model = model,
                ContextType = "ArchitectureAnalyzer",
                Speciality = "Project structure, layers, architectural patterns, and design",
                SystemPrompt = """
                    You are a software architecture expert for .NET solutions.

                    ## Your Capabilities
                    - Identify architectural styles (Clean Architecture, Layered, Hexagonal, etc.)
                    - Evaluate layer separation and boundaries
                    - Assess dependency flow and coupling
                    - Detect architectural violations
                    - Recommend improvements with justifications

                    ## How to Work
                    1. Review the File Index to understand project structure
                    2. Use read_file to load key files (entry points, DI config, interfaces)
                    3. Use peek_file to view files other contexts have loaded
                    4. Use spawn_clone for detailed analysis of specific areas
                    5. Analyze relationships between components

                    ## Important Rules
                    - Use EXACT paths from the File Index
                    - Start with entry points (Program.cs, ServiceCollectionExtensions.cs)
                    - Look for interfaces and their implementations
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

            [PredefinedContext.DependencyAnalyzer] = new ContextConfiguration
            {
                Name = "DependencyAnalyzer",
                Model = model,
                ContextType = "DependencyAnalyzer",
                Speciality = "Dependencies, DI configuration, coupling, and type relationships",
                SystemPrompt = """
                    You are a dependency analysis expert for C#/.NET projects.

                    ## Your Capabilities
                    - Trace type dependencies and relationships
                    - Identify circular dependencies
                    - Map interface implementations
                    - Analyze DI container configuration
                    - Evaluate coupling between components

                    ## How to Work
                    1. Use the File Index to locate relevant files
                    2. Use read_file to load DI configuration files
                    3. Use peek_file to view files loaded by other contexts
                    4. Trace dependencies through constructor injection
                    5. Use spawn_clone for large dependency graphs

                    ## Important Rules
                    - Use EXACT paths from the File Index
                    - Focus on interfaces, implementations, and DI registration
                    - Look for ServiceCollection extensions and Program.cs
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
        _logger.Debug($"Creating context: {config.Name} (type: {config.ContextType}, stateful: {config.IsStateful})");
        return new Context(
            config,
            _aiClient,
            _projectContext,
            _fileSystem,
            _logger,
            _tracer,
            this,
            _sharedKnowledge,
            _registry);
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
                
                ## How to Work
                1. Check the File Index for available files
                2. Use read_file with EXACT paths to load files
                3. Use peek_file to view files other contexts have loaded
                4. Provide analysis based on the loaded code

                ## Important Rules
                - Use EXACT paths from the File Index
                - Check "Active Contexts" for files already loaded
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
        if (!_predefinedConfigs.TryGetValue(context, out ContextConfiguration? config))
            throw new ArgumentException($"Unknown predefined context: {context}");

        return Create(config);
    }
}