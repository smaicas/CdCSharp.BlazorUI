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
                    Content = $"""
                        # Your Expertise Context
                        
                        The following files define your area of expertise. Study them carefully.
                        
                        {context}
                        
                        Confirm you have reviewed this context.
                        """
                });
                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.Assistant,
                    Content = $"""
                        I have reviewed my expertise context for "{spec.Expertise}".
                        
                        I analyzed {spec.InitialContextFiles.Count} file(s) and I'm ready to answer questions about:
                        - The code structure and patterns used
                        - Implementation details and dependencies
                        - How to extend or modify this code
                        
                        I will use the available tools when I need additional information.
                        """
                });
            }
        }

        _registry.Register(agent);
        _logger.Info($"Created agent: {agent.Name} ({agent.Id})");
        _logger.Debug($"  Expertise: {agent.Expertise}");
        _logger.Debug($"  Context files: {spec.InitialContextFiles.Count}");

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
        string toolsDocs = AITools.GetToolsDocumentation();

        return $"""
            # Agent Identity
            
            You are **{spec.Name}**, a specialized AI agent.
            
            **Your Expertise:** {spec.Expertise}
            
            You are part of a multi-agent system. The Orchestrator coordinates all agents and routes queries to the most appropriate specialist.
            
            ---
            
            # Available Tools
            
            You can use the following tools by including the exact syntax in your response:
            
            {toolsDocs}
            
            ---
            
            # Response Guidelines
            
            ## When to Use Tools
            
            1. **REQUEST_FILE**: Use when you need to see code you don't have in your context
               - Be specific with file paths
               - Request only files relevant to the current question
            
            2. **QUERY_AGENT**: Use when the question involves expertise outside your area
               - Clearly describe the expertise needed
               - Ask specific, focused questions
            
            3. **CREATE_AGENT**: Use sparingly, only when:
               - A specific area needs deep specialization
               - Multiple files need to be analyzed together
               - The current agents cannot answer adequately
            
            4. **GENERATE_FILE**: Use when creating new code or files
               - Always include the complete file content
               - Use appropriate language identifier
            
            5. **CONFIDENCE**: Always include at the end of your response
               - 0.9-1.0: Very confident, straightforward answer
               - 0.7-0.9: Confident but some assumptions made
               - 0.5-0.7: Moderate confidence, may need validation
               - Below 0.5: Low confidence, definitely needs validation
            
            6. **SUGGEST_VALIDATION**: Use when your answer spans multiple domains
            
            ---
            
            ## Response Format
            
            Structure your responses clearly:
            
            1. **Direct answer** to the question
            2. **Explanation** with relevant details
            3. **Code examples** if applicable (use GENERATE_FILE for new files)
            4. **Tool calls** if you need more information
            5. **Confidence score** at the end
            
            ---
            
            ## Important Rules
            
            - Do NOT invent or assume file contents you haven't seen
            - Do NOT claim expertise you don't have - use QUERY_AGENT instead
            - Be concise but thorough
            - If uncertain, state it clearly and lower your confidence
            - Always respond in the same language as the question
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
            string lang = Path.GetExtension(path).TrimStart('.') switch
            {
                "cs" => "csharp",
                "razor" => "razor",
                "ts" or "tsx" => "typescript",
                "css" or "scss" => "css",
                "json" => "json",
                _ => ""
            };

            parts.Add($$""""
                ## File: `{{path}}`
                ```{{lang}}
                {{content}}
                ```
                """");
        }

        return string.Join("\n\n---\n\n", parts);
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