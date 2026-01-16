using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CdCSharp.DocGen.Core.AI;

public class AiClientFactory : IAiClientFactory
{
    private readonly IOptions<DocGenOptions> _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPromptTracer _tracer;

    public AiClientFactory(
        IOptions<DocGenOptions> options,
        ILoggerFactory loggerFactory,
        IPromptTracer tracer)
    {
        _options = options;
        _loggerFactory = loggerFactory;
        _tracer = tracer;
    }

    public IAiClient Create()
    {
        AiProviderOptions aiOptions = _options.Value.Ai;

        return aiOptions.Provider switch
        {
            AiProviderType.LMStudio => new LMStudioClient(
                aiOptions.BaseUrl,
                _loggerFactory.CreateLogger<LMStudioClient>(),
                _tracer),

            AiProviderType.Groq => new GroqClient(
                aiOptions.ApiKey,
                aiOptions.Model,
                _loggerFactory.CreateLogger<GroqClient>(),
                _tracer),

            _ => throw new ArgumentException($"Unknown AI provider: {aiOptions.Provider}")
        };
    }
}