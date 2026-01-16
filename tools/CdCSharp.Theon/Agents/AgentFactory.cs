using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using CdCSharp.Theon.Tools;

namespace CdCSharp.Theon.Agents;

public class AgentFactory
{
    private readonly LMStudioClient _aiClient;
    private readonly FileAccessTool _fileAccess;
    private readonly AgentRegistry _registry;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;

    public AgentFactory(
        LMStudioClient aiClient,
        FileAccessTool fileAccess,
        AgentRegistry registry,
        TheonLogger logger,
        TheonOptions options)
    {
        _aiClient = aiClient;
        _fileAccess = fileAccess;
        _registry = registry;
        _logger = logger;
        _options = options;
    }

    public async Task<Agent> CreateAsync(AgentCreationSpec spec)
    {
        Agent agent = new()
        {
            Name = spec.Name,
            Expertise = spec.Expertise
        };

        string systemPrompt = BuildSystemPrompt(spec);
        agent.ConversationHistory.Add(new ConversationMessage
        {
            Role = MessageRole.System,
            Content = systemPrompt
        });

        if (spec.InitialContextFiles.Count > 0)
        {
            string context = await BuildInitialContextAsync(spec.InitialContextFiles);
            if (!string.IsNullOrEmpty(context))
            {
                agent.Context = context;
                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.User,
                    Content = $"Here is your expertise context:\n\n{context}"
                });
                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.Assistant,
                    Content = "I've reviewed my expertise context and I'm ready to help."
                });
            }
        }

        _registry.Register(agent);
        _logger.Info($"Created agent: {agent.Name} with expertise: {agent.Expertise}");

        return agent;
    }

    public void Sleep(Agent agent)
    {
        if (agent.State == AgentState.Sleeping) return;

        agent.SleepData = SerializeState(agent);
        agent.State = AgentState.Sleeping;
        agent.ConversationHistory.Clear();

        _logger.Debug($"Agent sleeping: {agent.Name}");
    }

    public void Wake(Agent agent)
    {
        if (agent.State == AgentState.Active) return;

        if (agent.SleepData != null)
        {
            RestoreState(agent, agent.SleepData);
            agent.SleepData = null;
        }

        agent.State = AgentState.Active;
        agent.LastActiveAt = DateTime.UtcNow;

        _logger.Debug($"Agent woken: {agent.Name}");
    }

    private string BuildSystemPrompt(AgentCreationSpec spec)
    {
        return $"""
            You are a specialized AI agent named "{spec.Name}".
            
            Your expertise: {spec.Expertise}
            
            ## Your Capabilities
            
            You can request actions from the orchestrator by including these tags in your response:
            
            1. Request file content:
               [REQUEST_FILE: path="relative/path/to/file.cs"]
            
            2. Request to query another agent:
               [QUERY_AGENT: expertise="area of expertise" question="your question"]
            
            3. Request creation of a new specialized agent:
               [CREATE_AGENT: name="Agent Name" expertise="specific expertise" files="file1.cs,file2.cs"]
            
            ## Response Guidelines
            
            - Be precise and focused on your expertise area
            - If you need information outside your context, request it
            - Include a confidence score (0.0-1.0) at the end: [CONFIDENCE: 0.85]
            - If generating code, wrap it in appropriate markdown code blocks
            - Suggest validation by other agents if the topic spans multiple areas:
               [SUGGEST_VALIDATION: expertise="area1,area2"]
            
            ## Output Format for Generated Files
            
            When generating files, use this format:
            [FILE: name="FileName.cs" language="csharp"]

            // file content here

            [/FILE]
            """;
    }

    private async Task<string> BuildInitialContextAsync(List<string> files)
    {
        Dictionary<string, string> contents = await _fileAccess.GetFilesContentAsync(files);

        if (contents.Count == 0)
            return string.Empty;

        List<string> parts = [];
        foreach ((string path, string content) in contents)
        {
            parts.Add($"=== {path} ===\n{content}");
        }

        return string.Join("\n\n", parts);
    }

    private static byte[] SerializeState(Agent agent)
    {
        string state = System.Text.Json.JsonSerializer.Serialize(new
        {
            agent.Context,
            History = agent.ConversationHistory.ToList()
        });
        return System.Text.Encoding.UTF8.GetBytes(state);
    }

    private static void RestoreState(Agent agent, byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        AgentSleepState? state = System.Text.Json.JsonSerializer.Deserialize<AgentSleepState>(json);

        if (state != null)
        {
            agent.Context = state.Context;
            agent.ConversationHistory.Clear();
            agent.ConversationHistory.AddRange(state.History);
        }
    }

    private record AgentSleepState
    {
        public string Context { get; init; } = "";
        public List<ConversationMessage> History { get; init; } = [];
    }
}