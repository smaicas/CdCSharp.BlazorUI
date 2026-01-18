using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestration;
using CdCSharp.Theon.Tools;
using CdCSharp.Theon.Tools.Exploration;
using CdCSharp.Theon.Tools.Modification;
using CdCSharp.Theon.Tools.Output;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.Theon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTheon(this IServiceCollection services, TheonOptions options)
    {
        EnsureDirectories(options);

        services.AddSingleton(options);

        // Infrastructure
        services.AddSingleton<ITheonLogger, TheonLogger>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IOutputContext, OutputContext>();

        // Tools - registered via IEnumerable<ITool>
        services.AddSingleton<ITool, ExploreAssemblyTool>();
        services.AddSingleton<ITool, ExploreFileTool>();
        services.AddSingleton<ITool, ExploreFolderTool>();
        services.AddSingleton<ITool, ExploreFilesTool>();
        services.AddSingleton<ITool, GenerateFileTool>();
        services.AddSingleton<ITool, AppendFileTool>();
        services.AddSingleton<ITool, OverwriteFileTool>();
        services.AddSingleton<ITool, ModifyProjectFileTool>();

        // Tool Registry
        services.AddSingleton<IToolRegistry, ToolRegistry>();

        // LLM Communication
        services.AddSingleton<ILlmClient, LlmClient>();
        services.AddSingleton<IResponseParser, ResponseParser>();
        services.AddSingleton<IPromptBuilder, PromptBuilder>();

        // Analysis
        services.AddSingleton<IProjectAnalysis, ProjectAnalysis>();

        // Context
        services.AddSingleton<IScopeFactory, ScopeFactory>();

        // Quality Layer
        services.AddSingleton<IResponseValidator, ResponseValidator>();
        services.AddSingleton<IExplorationStrategies, ExplorationStrategies>();
        services.AddSingleton<IOutputPlanner, OutputPlanner>();

        // Orchestration
        services.AddSingleton<IOrchestrator, Orchestrator>();

        return services;
    }

    private static void EnsureDirectories(TheonOptions options)
    {
        string basePath = Path.IsPathRooted(options.OutputPath)
            ? options.OutputPath
            : Path.Combine(options.ProjectPath, options.OutputPath);

        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(Path.Combine(basePath, "responses"));
        Directory.CreateDirectory(Path.Combine(basePath, "logs"));

        if (options.Modification.CreateBackup)
            Directory.CreateDirectory(Path.Combine(basePath, "backups"));
    }
}