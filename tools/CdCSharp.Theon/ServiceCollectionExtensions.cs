using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
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

        //// Infrastructure
        services.AddSingleton<ITheonLogger, TheonLogger>();
        services.AddSingleton<IAIClient, LMStudioClient>();
        //services.AddSingleton<IFileSystem, FileSystem>();
        //services.AddSingleton<IOutputContext, OutputContext>();

        //// Tools - registered via IEnumerable<ITool>
        //services.AddSingleton<ITool, ExploreAssemblyTool>();
        //services.AddSingleton<ITool, ExploreFileTool>();
        //services.AddSingleton<ITool, ExploreFolderTool>();
        //services.AddSingleton<ITool, ExploreFilesTool>();
        //services.AddSingleton<ITool, GenerateFileTool>();
        //services.AddSingleton<ITool, AppendFileTool>();
        //services.AddSingleton<ITool, OverwriteFileTool>();
        //services.AddSingleton<ITool, ModifyProjectFileTool>();

        //// Tool Registry
        //services.AddSingleton<IToolRegistry, ToolRegistry>();

        //// LLM Communication
        //services.AddSingleton<ILlmClient, LlmClient>();
        //services.AddSingleton<IResponseParser, ResponseParser>();
        //services.AddSingleton<IPromptBuilder, PromptBuilder>();

        //// Analysis
        //services.AddSingleton<IProjectAnalysis, ProjectAnalysis>();

        //// Context
        //services.AddSingleton<IScopeFactory, ScopeFactory>();

        //// Quality Layer
        //services.AddSingleton<IResponseValidator, ResponseValidator>();
        //services.AddSingleton<IExplorationStrategies, ExplorationStrategies>();
        //services.AddSingleton<IOutputPlanner, OutputPlanner>();

        //// Orchestration
        //services.AddSingleton<IOrchestrator, Orchestrator>();

        return services;
    }

    internal sealed class TheonOptionsValidator : IValidateOptions<TheonOptions>
    {
        public ValidateOptionsResult Validate(string? name, TheonOptions options)
        {
            List<string> errors = [];

            if (string.IsNullOrWhiteSpace(options.ProjectPath))
            {
                errors.Add("ProjectPath es requerido.");
            }

            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                errors.Add("OutputPath es requerido.");
            }

            if (options.Validation.LowConfidenceThreshold is < 0 or > 1)
            {
                errors.Add("LowConfidenceThreshold debe estar entre 0 y 1.");
            }

            if (options.Validation.MaxValidationRetries < 0)
            {
                errors.Add("MaxValidationRetries debe ser mayor o igual a 0.");
            }

            if (options.Llm.TimeoutSeconds <= 0)
            {
                errors.Add("TimeoutSeconds debe ser mayor que 0.");
            }

            if (options.Llm.Temperature is < 0 or > 2)
            {
                errors.Add("Temperature debe estar entre 0 y 2.");
            }

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }

    internal sealed class TheonOptionsSetup : IPostConfigureOptions<TheonOptions>
    {
        public void PostConfigure(string? name, TheonOptions options)
        {
            EnsureDirectories(options);

            static void EnsureDirectories(TheonOptions options)
            {
                string basePath = Path.IsPathRooted(options.OutputPath)
                    ? options.OutputPath
                    : Path.Combine(options.ProjectPath, options.OutputPath);

                Directory.CreateDirectory(basePath);
                Directory.CreateDirectory(options.ResponsesPath);
                Directory.CreateDirectory(options.LogsPath);

                if (options.Modification.CreateBackup)
                {
                    Directory.CreateDirectory(options.BackupsPath);
                }
            }

        }
    }
}