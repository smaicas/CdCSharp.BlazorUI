using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CdCSharp.Theon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTheon(this IServiceCollection services, Action<TheonOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IValidateOptions<TheonOptions>, TheonOptionsValidator>();
        services.AddSingleton<IPostConfigureOptions<TheonOptions>, TheonOptionsSetup>();

        services.AddSingleton<ITheonLogger, TheonLogger>();
        services.AddSingleton<IFileSystem>(sp =>
        {
            TheonOptions options = sp.GetRequiredService<IOptions<TheonOptions>>().Value;
            ITheonLogger logger = sp.GetRequiredService<ITheonLogger>();
            return new FileSystem(options, logger);
        });

        services.AddSingleton<IAIClient, LMStudioClient>();

        services.AddSingleton<IProjectContext, ProjectContext>();
        services.AddSingleton<SharedProjectKnowledge>();

        services.AddSingleton<ContextRegistry>();
        services.AddSingleton<ContextBudgetManager>();
        services.AddSingleton<PromptFormatter>();
        services.AddSingleton<IContextFactory, ContextFactory>();

        services.AddSingleton<IOrchestrator, Orchestrator.Orchestrator>();

        return services;
    }

    internal sealed class TheonOptionsValidator : IValidateOptions<TheonOptions>
    {
        public ValidateOptionsResult Validate(string? name, TheonOptions options)
        {
            List<string> errors = [];

            if (string.IsNullOrWhiteSpace(options.ProjectPath))
                errors.Add("ProjectPath is required.");

            if (string.IsNullOrWhiteSpace(options.OutputPath))
                errors.Add("OutputPath is required.");

            if (options.Llm.TimeoutSeconds <= 0)
                errors.Add("TimeoutSeconds must be > 0.");

            if (options.Llm.Temperature is < 0 or > 2)
                errors.Add("Temperature must be between 0 and 2.");

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }

    internal sealed class TheonOptionsSetup : IPostConfigureOptions<TheonOptions>
    {
        public void PostConfigure(string? name, TheonOptions options)
        {
            string basePath = Path.IsPathRooted(options.OutputPath)
                ? options.OutputPath
                : Path.Combine(options.ProjectPath, options.OutputPath);

            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "responses"));
            Directory.CreateDirectory(Path.Combine(basePath, "logs"));
            Directory.CreateDirectory(Path.Combine(basePath, "traces"));

            if (options.Modification.CreateBackup)
            {
                Directory.CreateDirectory(Path.Combine(basePath, "backups"));
            }
        }
    }
}