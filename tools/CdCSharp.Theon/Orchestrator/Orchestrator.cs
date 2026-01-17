using CdCSharp.Theon.Agents;
using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using CdCSharp.Theon.Tools;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CdCSharp.Theon.Orchestration;

public class Orchestrator
{
    private readonly LMStudioClient _aiClient;
    private readonly AgentRegistry _registry;
    private readonly AgentFactory _agentFactory;
    private readonly AgentExecutor _agentExecutor;
    private readonly FileAccessTool _fileAccess;
    private readonly FileOutputTool _fileOutput;
    private readonly LlmFormatter _formatter;
    private readonly TheonLogger _logger;
    private readonly TheonOptions _options;

    private readonly List<ConversationMessage> _orchestratorHistory = [];

    public Orchestrator(
        LMStudioClient aiClient,
        AgentRegistry registry,
        AgentFactory agentFactory,
        AgentExecutor agentExecutor,
        FileAccessTool fileAccess,
        FileOutputTool fileOutput,
        LlmFormatter formatter,
        TheonLogger logger,
        TheonOptions options)
    {
        _aiClient = aiClient;
        _registry = registry;
        _agentFactory = agentFactory;
        _agentExecutor = agentExecutor;
        _fileAccess = fileAccess;
        _fileOutput = fileOutput;
        _formatter = formatter;
        _logger = logger;
        _options = options;

        InitializeOrchestratorPrompt();
    }

    public void SetProjectStructure(PreAnalysisResult preAnalysis)
    {
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"Project structure loaded:\n\n{preAnalysis.ProjectLlmFormat}"
        });
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = "I've analyzed the project structure. Ready to process queries."
        });
    }

    public async Task<ResponseOutput> ProcessQueryAsync(string query)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> involvedAgents = [];
        List<GeneratedFile> allFiles = [];
        float finalConfidence = 1.0f;
        int validationRounds = 0;

        _logger.Info($"Processing query: {query}");

        RoutingDecision routing = await DecideRoutingAsync(query);
        _logger.Debug($"Routing decision: {routing.Strategy} -> {routing.TargetExpertise}");

        Agent agent = await GetOrCreateAgentAsync(routing.TargetExpertise, routing.SuggestedFiles);
        involvedAgents.Add(agent.Name);

        string instruction = BuildAgentInstruction(query, routing);
        AgentExecutionResult result = await ExecuteWithRequestsAsync(agent, instruction, involvedAgents);

        allFiles.AddRange(result.GeneratedFiles);
        finalConfidence = result.Confidence;

        if (result.Confidence < _options.Validation.ConfidenceThreshold || result.SuggestedValidators.Count > 0)
        {
            (string validatedContent, float validatedConfidence, int rounds) =
                await ValidateResponseAsync(result, involvedAgents);

            result.CleanContent = validatedContent;
            finalConfidence = validatedConfidence;
            validationRounds = rounds;
        }

        stopwatch.Stop();

        ResponseMetadata metadata = new()
        {
            AgentsInvolved = involvedAgents,
            ValidationRounds = validationRounds,
            FinalConfidence = finalConfidence,
            ProcessingTime = stopwatch.Elapsed
        };

        return await _fileOutput.SaveResponseAsync(query, result.CleanContent, allFiles, metadata);
    }

    private async Task<AgentExecutionResult> ExecuteWithRequestsAsync(
        Agent agent,
        string instruction,
        List<string> involvedAgents,
        int depth = 0)
    {
        if (depth > 5)
        {
            _logger.Warning("Max recursion depth reached");
            return new AgentExecutionResult { CleanContent = "Max depth reached" };
        }

        AgentExecutionResult result = await _agentExecutor.ExecuteAsync(agent, instruction);

        while (result.HasPendingRequests)
        {
            StringBuilder additionalContext = new();

            if (result.FileRequests.Count > 0)
            {
                _logger.Debug($"Agent requested {result.FileRequests.Count} files");
                Dictionary<string, string> files = await _fileAccess.GetFilesContentAsync(result.FileRequests);

                foreach ((string path, string content) in files)
                {
                    additionalContext.AppendLine($"=== {path} ===");
                    additionalContext.AppendLine(content);
                    additionalContext.AppendLine();
                }
            }

            foreach (AgentCreationSpec spec in result.AgentCreationRequests)
            {
                _logger.Info($"Creating sub-agent: {spec.Name}");
                Agent subAgent = await _agentFactory.CreateAsync(spec);
                involvedAgents.Add(subAgent.Name);
            }

            foreach (AgentRequest queryReq in result.AgentQueries)
            {
                _logger.Debug($"Cross-agent query to: {queryReq.TargetExpertise}");

                Agent? targetAgent = _registry.FindByExpertise(queryReq.TargetExpertise!);
                if (targetAgent == null)
                {
                    targetAgent = await _agentFactory.CreateAsync(new AgentCreationSpec
                    {
                        Name = $"{queryReq.TargetExpertise} Specialist",
                        Expertise = queryReq.TargetExpertise!
                    });
                }

                if (targetAgent.State == AgentState.Sleeping)
                    _agentFactory.Wake(targetAgent);

                involvedAgents.Add(targetAgent.Name);

                AgentExecutionResult subResult = await ExecuteWithRequestsAsync(
                    targetAgent, queryReq.Payload, involvedAgents, depth + 1);

                additionalContext.AppendLine($"Response from {targetAgent.Name}:");
                additionalContext.AppendLine(subResult.CleanContent);
                additionalContext.AppendLine();
            }

            if (additionalContext.Length > 0)
            {
                string continuation = $"""
                    Here is the additional information you requested:
                    
                    {additionalContext}
                    
                    Please continue your response with this new context.
                    """;

                result = await _agentExecutor.ExecuteAsync(agent, continuation);
            }
            else
            {
                break;
            }
        }
        return result;
    }
    private async Task<(string Content, float Confidence, int Rounds)> ValidateResponseAsync(
        AgentExecutionResult originalResult,
        List<string> involvedAgents)
    {
        string content = originalResult.CleanContent;
        float confidence = originalResult.Confidence;
        int rounds = 0;

        List<string> validatorExpertise = originalResult.SuggestedValidators.Count > 0
            ? originalResult.SuggestedValidators
            : ["code review", "architecture"];

        for (int i = 0; i < _options.Validation.MaxIterations; i++)
        {
            rounds++;
            bool allApproved = true;
            List<string> allSuggestions = [];

            foreach (string expertise in validatorExpertise.Take(2))
            {
                Agent? validator = _registry.FindByExpertise(expertise);
                if (validator == null || validator.Id == originalResult.AgentId)
                    continue;

                if (validator.State == AgentState.Sleeping)
                    _agentFactory.Wake(validator);

                involvedAgents.Add(validator.Name);

                string validationPrompt = $"""
                Please review this response for accuracy and completeness:
                
                {content}
                
                Respond with:
                - [APPROVED] if the response is correct
                - [OBJECTION: reason] if there are issues
                - [SUGGESTION: improvement] for enhancements
                
                Then provide your [CONFIDENCE: 0.0-1.0]
                """;

                AgentExecutionResult valResult = await _agentExecutor.ExecuteAsync(validator, validationPrompt);

                if (valResult.CleanContent.Contains("[OBJECTION", StringComparison.OrdinalIgnoreCase))
                {
                    allApproved = false;
                    allSuggestions.Add(valResult.CleanContent);
                }

                confidence = Math.Min(confidence, valResult.Confidence);
            }

            if (allApproved)
            {
                _logger.Debug($"Validation approved after {rounds} rounds");
                break;
            }

            if (allSuggestions.Count > 0 && i < _options.Validation.MaxIterations - 1)
            {
                Agent originalAgent = _registry.Get(originalResult.AgentId)!;
                string refinementPrompt = $"""
                Your response received feedback:
                
                {string.Join("\n\n", allSuggestions)}
                
                Please refine your response addressing these points.
                """;

                AgentExecutionResult refined = await _agentExecutor.ExecuteAsync(originalAgent, refinementPrompt);
                content = refined.CleanContent;
                confidence = refined.Confidence;
            }
        }

        return (content, confidence, rounds);
    }

    private async Task<RoutingDecision> DecideRoutingAsync(string query)
    {
        string existingAgents = _registry.GetAgentsSummary();

        string routingPrompt = $$"""
        Analyze this query and decide how to handle it:
        
        Query: {{query}}
        
        {{existingAgents}}
        
        Respond with JSON only:
        {
            "strategy": "existing|new|direct",
            "targetExpertise": "expertise description",
            "suggestedFiles": ["file1.cs", "file2.cs"],
            "reasoning": "brief explanation"
        }
        
        - Use "existing" if an existing agent can handle it
        - Use "new" if a new specialized agent is needed
        - Use "direct" only for very simple queries to an stateless agent
        """;

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = routingPrompt
        });

        RoutingDecision? decision = await _aiClient.SendAsync<RoutingDecision>(
            _orchestratorHistory, maxTokens: 500);

        _logger.LogOrchestratorInteraction(InteractionDirection.Input, string.Join("\n\n", _orchestratorHistory));
        _logger.LogOrchestratorInteraction(InteractionDirection.Output, JsonSerializer.Serialize(decision));

        if (decision == null)
        {
            return new RoutingDecision
            {
                Strategy = "new",
                TargetExpertise = "general code analysis",
                Reasoning = "Fallback routing"
            };
        }

        return decision;
    }

    private async Task<Agent> GetOrCreateAgentAsync(string expertise, List<string>? files = null)
    {
        Agent? existing = _registry.FindByExpertise(expertise);

        if (existing != null)
        {
            if (existing.State == AgentState.Sleeping)
                _agentFactory.Wake(existing);

            return existing;
        }

        return await _agentFactory.CreateAsync(new AgentCreationSpec
        {
            Name = $"{expertise} Specialist",
            Expertise = expertise,
            InitialContextFiles = files ?? []
        });
    }

    private string BuildAgentInstruction(string query, RoutingDecision routing)
    {
        return $"""
        User query: {query}
        
        Context: {routing.Reasoning}
        
        Please provide a comprehensive response. Remember:
        - Request files if you need to see specific code
        - Query other agents if you need expertise outside your area
        - Suggest validation if the response involves multiple domains
        - Include your confidence score
        """;
    }

    private void InitializeOrchestratorPrompt()
    {
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.System,
            Content = """
            You are the Orchestrator for a multi-agent code analysis system.
            
            Your responsibilities:
            1. Analyze incoming queries and route them to appropriate agents
            2. Decide when to create new specialized agents
            3. Coordinate validation between agents
            4. Manage agent lifecycle (create, wake, sleep)
            
            You have access to:
            - Project structure information
            - List of existing agents and their expertise
            - File access tools
            
            Always respond with valid JSON when making routing decisions.
            """
        });
    }
}

public record RoutingDecision
{
    public string Strategy { get; init; } = "new";
    public string TargetExpertise { get; init; } = "";
    public List<string> SuggestedFiles { get; init; } = [];
    public string Reasoning { get; init; } = "";
}