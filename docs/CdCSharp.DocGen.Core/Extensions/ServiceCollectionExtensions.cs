using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using CdCSharp.DocGen.Core.Agents;
using CdCSharp.DocGen.Core.AI;
using CdCSharp.DocGen.Core.Analysis;
using CdCSharp.DocGen.Core.Cache;
using CdCSharp.DocGen.Core.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.DocGen.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocGen(
        this IServiceCollection services,
        Action<DocGenOptions> configure)
    {
        DocGenOptions options = new();
        configure(options);

        services.Configure<DocGenOptions>(opt =>
        {
            opt.ProjectPath = options.ProjectPath;
            opt.OutputPath = options.OutputPath;
            opt.Ai = options.Ai;
            opt.Cache = options.Cache;
            opt.PromptTracer = options.PromptTracer;
            opt.Conversation = options.Conversation;
        });

        services.AddSingleton<IIgnoreFilter, IgnoreFilter>();
        services.AddSingleton<IPromptTracer, PromptTracer>();

        services.AddScoped<IAssemblyScanner, AssemblyScanner>();
        services.AddScoped<ITypeDestructurer, TypeDestructurer>();
        services.AddScoped<IComponentAnalyzer, ComponentAnalyzer>();
        services.AddScoped<ITypeScriptAnalyzer, TypeScriptAnalyzer>();
        services.AddScoped<ICssAnalyzer, CssAnalyzer>();
        services.AddScoped<IProjectAnalyzer, ProjectAnalyzer>();

        services.AddSingleton<IAiClientFactory, AiClientFactory>();
        services.AddScoped<IAiClient>(sp =>
            sp.GetRequiredService<IAiClientFactory>().Create());

        if (options.Cache.Enabled)
            services.AddSingleton<ICacheManager, CacheManager>();
        else
            services.AddSingleton<ICacheManager, NullCacheManager>();

        services.AddSingleton<IAgentRegistry, AgentRegistry>();
        services.AddScoped<AgentFactory>();
        services.AddScoped<IAgentFactory>(sp => sp.GetRequiredService<AgentFactory>());
        services.AddScoped<IExpertiseContextBuilder, ExpertiseContextBuilder>();
        services.AddScoped<IOrchestrator, Orchestrator>();

        services.AddScoped<IPlainTextFormatter, PlainTextFormatter>();
        services.AddScoped<IHumanDocComposer, HumanDocComposer>();
        services.AddScoped<ILlmDocComposer, LlmDocComposer>();

        services.AddScoped<DocGenRunner>();

        return services;
    }
}