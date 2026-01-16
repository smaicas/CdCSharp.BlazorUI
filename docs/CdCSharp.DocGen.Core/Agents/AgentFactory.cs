using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CdCSharp.DocGen.Core.Agents;

public class AgentFactory : IAgentFactory
{
    private readonly IAiClient _aiClient;
    private readonly ILogger<Agent> _agentLogger;
    private readonly ConversationOptions _conversationOptions;
    private Func<AgentQuery, Task<AgentQueryResult>>? _queryHandler;

    public AgentFactory(
        IAiClient aiClient,
        IOptions<DocGenOptions> options,
        ILogger<Agent> agentLogger)
    {
        _aiClient = aiClient;
        _agentLogger = agentLogger;
        _conversationOptions = options.Value.Conversation;
    }

    public void SetQueryHandler(Func<AgentQuery, Task<AgentQueryResult>> handler)
    {
        _queryHandler = handler;
    }

    public IAgent Create(AgentDefinition definition)
    {
        if (_queryHandler == null)
            throw new InvalidOperationException("Query handler not set. Call SetQueryHandler first.");

        return new Agent(definition, _aiClient, _queryHandler, _agentLogger, _conversationOptions);
    }

    public AgentDefinition BuildDefinition(AgentCreationRequest request)
    {
        string id = GenerateAgentId(request.Name);

        string systemPrompt = $"""
            You are a {request.Name}.
            {request.Description}
            
            IMPORTANT: If you need information about specific code areas outside your expertise,
            you can request it by responding with:
            [QUERY_AGENT: expertise="topic" question="your question"]
            
            The orchestrator will route your query to the appropriate specialist.
            """;

        return new AgentDefinition
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            SystemPrompt = systemPrompt,
            Expertise = request.Expertise,
            Capabilities = request.Expertise.Topics,
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateAgentId(string name)
    {
        string normalized = name.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");

        string suffix = Guid.NewGuid().ToString("N")[..4];

        return $"{normalized}_{suffix}";
    }
}