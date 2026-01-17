using CdCSharp.Theon.Agents;
using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Orchestration;
using CdCSharp.Theon.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.Theon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTheon(this IServiceCollection services, TheonOptions options)
    {
        services.AddSingleton(options);

        string outputPath = Path.IsPathRooted(options.OutputPath)
            ? options.OutputPath
            : Path.Combine(options.ProjectPath, options.OutputPath);

        Directory.CreateDirectory(outputPath);

        TheonLogger logger = new(outputPath, LogLevel.Debug);
        services.AddSingleton(logger);

        MetricsCollector metrics = new(logger);
        services.AddSingleton(metrics);

        GeneratedFilesTracker filesTracker = new(logger);
        services.AddSingleton(filesTracker);

        IgnoreFilter ignoreFilter = new();
        services.AddSingleton(ignoreFilter);

        LMStudioClient aiClient = new(
            options.LMStudio.BaseUrl,
            options.LMStudio.TimeoutSeconds,
            logger,
            metrics);
        services.AddSingleton(aiClient);

        services.AddSingleton<PreAnalyzer>();
        services.AddSingleton<TypeDestructurer>();
        services.AddSingleton<LlmFormatter>();

        FileAccessTool fileAccess = new(options.ProjectPath, ignoreFilter, logger);
        services.AddSingleton(fileAccess);

        AgentRegistry registry = new(logger);
        services.AddSingleton(registry);

        AgentVisualizer visualizer = new(registry);
        services.AddSingleton(visualizer);

        FileOutputTool fileOutput = new(outputPath, logger, visualizer);
        services.AddSingleton(fileOutput);

        SessionManager sessionManager = new(outputPath, logger);
        services.AddSingleton(sessionManager);

        CompressionAgent compressionAgent = new(aiClient, logger);
        services.AddSingleton(compressionAgent);

        AgentFactory agentFactory = new(aiClient, fileAccess, registry, logger, options);
        services.AddSingleton(agentFactory);

        AgentExecutor agentExecutor = new(aiClient, logger, options, compressionAgent, filesTracker);
        services.AddSingleton(agentExecutor);

        services.AddSingleton<Orchestrator>();

        return services;
    }
}