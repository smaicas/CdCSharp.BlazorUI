using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestration;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.Theon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTheon(this IServiceCollection services, TheonOptions options)
    {
        EnsureDirectories(options);

        // Core configuration
        services.AddSingleton(options);

        // Infrastructure
        services.AddSingleton<ITheonLogger, TheonLogger>();
        services.AddSingleton<IFileSystem, FileSystem>();

        // LLM Communication
        services.AddSingleton<ILlmClient, LlmClient>();
        services.AddSingleton<IToolParser, ToolParser>();

        // Analysis
        services.AddSingleton<IProjectAnalysis, ProjectAnalysis>();

        // Context Management
        services.AddSingleton<IScopeFactory, ScopeFactory>();

        // NEW: Quality & Intelligence Layer
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