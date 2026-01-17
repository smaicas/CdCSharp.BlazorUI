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

        IgnoreFilter ignoreFilter = new();
        services.AddSingleton(ignoreFilter);

        LMStudioClient aiClient = new(
            options.LMStudio.BaseUrl,
            options.LMStudio.TimeoutSeconds,
            logger);
        services.AddSingleton(aiClient);

        services.AddSingleton<PreAnalyzer>();
        services.AddSingleton<TypeDestructurer>();
        services.AddSingleton<LlmFormatter>();

        FileAccessTool fileAccess = new(options.ProjectPath, ignoreFilter, logger);
        services.AddSingleton(fileAccess);

        FileOutputTool fileOutput = new(outputPath, logger);
        services.AddSingleton(fileOutput);

        AgentRegistry registry = new(logger);
        services.AddSingleton(registry);

        AgentFactory agentFactory = new(aiClient, fileAccess, registry, logger, options);
        services.AddSingleton(agentFactory);

        AgentExecutor agentExecutor = new(aiClient, logger, options);
        services.AddSingleton(agentExecutor);

        services.AddSingleton<Orchestrator>();

        return services;
    }
}