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
        // Validar spec antes de crear
        if (string.IsNullOrWhiteSpace(spec.Name))
            throw new ArgumentException("Agent name cannot be empty", nameof(spec));

        if (string.IsNullOrWhiteSpace(spec.Expertise))
            throw new ArgumentException("Agent expertise cannot be empty", nameof(spec));

        Agent agent = new()
        {
            Name = spec.Name.Trim(),
            Expertise = spec.Expertise.Trim()
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
                    # Context Files
                    
                    Study these files - they define your area of expertise:
                    
                    {context}
                    
                    Confirm you understand this context.
                    """
                });
                agent.ConversationHistory.Add(new ConversationMessage
                {
                    Role = MessageRole.Assistant,
                    Content = $"I have analyzed {spec.InitialContextFiles.Count} file(s) for my expertise in \"{spec.Expertise}\". Ready to answer questions."
                });
            }
        }

        _registry.Register(agent);
        _logger.LogAgentCreation(agent.Id, agent.Name, agent.Expertise);

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

        return $$"""
You are {{spec.Name}}.
Your expertise: {{spec.Expertise}}
You are part of THEON, a multi-agent code analysis system.
The Orchestrator routes queries to you based on your expertise.

TOOLS
{{toolsDocs}}

BLOCK TAG FORMAT
Opening tag, then content, then closing tag. Example:
[GENERATE_FILE: name="Example.cs" language="csharp"]
public class Example { }
[/GENERATE_FILE]

The closing tag must exist. Without it, the content will not be captured.
Do not nest block tags inside other block tags.

RESPONSE REQUIREMENTS
1. You must include [CONFIDENCE: X.X] at the end of every response.
2. Confidence scale:
   0.9-1.0 = complete answer, no gaps
   0.7-0.9 = good answer with minor assumptions
   0.5-0.7 = incomplete, needs validation
   below 0.5 = needs review
3. Use [TASK_COMPLETE] when you have fully addressed the query.

RULES
- Do not invent file contents. Use REQUEST_FILE to see files you need.
- Do not assume expertise outside your domain. Use QUERY_AGENT to consult other specialists.
- Respond in the same language as the query.
- If you generate files, they will be written to disk exactly as you provide them between the tags.
""";
    }

    //    private string BuildSystemPrompt(AgentCreationSpec spec)
    //{
    //    return $"""
    //        # You are {spec.Name}

    //        Your expertise: {spec.Expertise}

    //        You are part of THEON, a multi-agent code analysis system.

    //        ## How to Respond

    //        1. Answer the user's question directly
    //        2. If you need to see a file, use: [REQUEST_FILE: path="path/to/file.cs"]
    //        3. If you need the file list, use: [REQUEST_FILE_PATHS: assembly=""]
    //        4. If you need another expert, use: [QUERY_AGENT: expertise="area" question="your question"]
    //        5. To generate a file, use:
    //           [GENERATE_FILE: name="File.cs" language="csharp"]

    //           // code here

    //           [/GENERATE_FILE]
    //        6. Always end with: [CONFIDENCE: 0.0-1.0]

    //        ## Rules

    //        - Never invent file contents - request files you need
    //        - Be concise but complete
    //        - Respond in the same language as the question
    //        - If uncertain, lower your confidence score

    //        ## Confidence Scale

    //        - 0.9-1.0: Certain
    //        - 0.7-0.9: Confident
    //        - 0.5-0.7: Moderate
    //        - Below 0.5: Needs review
    //        """;
    //}

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
                ## File: {{path}}
                ## Lang: {{lang}}
                ## Content:

                {{content}}

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