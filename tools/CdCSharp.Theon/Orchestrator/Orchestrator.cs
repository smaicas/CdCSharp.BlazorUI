// Orchestrator/Orchestrator.cs
using CdCSharp.Theon.Agents;
using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using CdCSharp.Theon.Tools;
using System.Diagnostics;
using System.Text;

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
    private readonly SessionManager _sessionManager;
    private readonly MetricsCollector _metrics;

    private string _projectFileList = string.Empty;
    private PreAnalysisResult? _preAnalysis;
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
        SessionManager sessionManager,
        MetricsCollector metrics,
        TheonOptions options)
    {
        _aiClient = aiClient;
        _registry = registry;
        _agentFactory = agentFactory;
        _agentExecutor = agentExecutor;
        _fileAccess = fileAccess;
        _fileOutput = fileOutput;
        _formatter = formatter;
        _sessionManager = sessionManager;
        _metrics = metrics;
        _logger = logger;
        _options = options;

        InitializeOrchestratorPrompt();
    }

    public void SetProjectStructure(PreAnalysisResult preAnalysis)
    {
        _preAnalysis = preAnalysis;

        string projectOverview = _formatter.FormatProjectStructure(preAnalysis.Structure);
        string fileList = BuildFileListCompact(preAnalysis);

        // Dar contexto completo del proyecto al orquestador
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"""
            # Project Loaded
            
            You are now analyzing this project:
            
            ## Project Info
            - Name: {preAnalysis.Structure.Solution}
            - Type: {preAnalysis.Structure.Summary.ProjectType}
            - Assemblies: {preAnalysis.Structure.Summary.TotalAssemblies}
            - Types: {preAnalysis.Structure.Summary.TotalTypes}
            - Files: {preAnalysis.Structure.Summary.TotalFiles}
            
            ## Structure
            {projectOverview}
            
            ## Files
            {fileList}
            
            Confirm you understand the project.
            """
        });

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content =
            $$"""
            {"understood":true,"project":"{{preAnalysis.Structure.Solution}}","ready":true}
            """
        });
    }

    private static string BuildFileListCompact(PreAnalysisResult preAnalysis)
    {
        List<string> allFiles = preAnalysis.Structure.Assemblies
            .Where(a => !a.IsTestProject)
            .SelectMany(a => a.Files.CSharp
                .Concat(a.Files.Razor)
                .Concat(a.Files.TypeScript))
            .ToList();

        return string.Join("\n", allFiles.Select(f => $"- {f}"));
    }

    public SessionState CreateSessionState()
    {
        List<SerializedAgentState> agentStates = _registry.AllAgents.Select(a => new SerializedAgentState
        {
            Id = a.Id,
            Name = a.Name,
            Expertise = a.Expertise,
            Context = a.Context,
            State = a.State,
            ConversationHistory = a.ConversationHistory.ToList(),
            CreatedAt = a.CreatedAt,
            LastActiveAt = a.LastActiveAt
        }).ToList();

        return new SessionState
        {
            ProjectPath = _options.ProjectPath,
            Agents = agentStates,
            OrchestratorHistory = _orchestratorHistory.ToList(),
            Metrics = _metrics.GetSummary()
        };
    }

    public async Task<string> SaveSessionAsync()
    {
        SessionState state = CreateSessionState();
        return await _sessionManager.SaveSessionAsync(state);
    }

    public async Task<bool> LoadSessionAsync(string sessionIdOrPath)
    {
        SessionState? state = await _sessionManager.LoadSessionAsync(sessionIdOrPath);

        if (state == null)
            return false;

        _sessionManager.RestoreAgents(state, _registry, _agentFactory);

        _orchestratorHistory.Clear();
        _orchestratorHistory.AddRange(state.OrchestratorHistory);

        _logger.Info($"Session restored with {state.Agents.Count} agents");
        return true;
    }

    private static string BuildFileList(PreAnalysisResult preAnalysis)
    {
        List<string> lines = [];

        foreach (AssemblyStructure assembly in preAnalysis.Structure.Assemblies)
        {
            if (assembly.IsTestProject) continue;

            lines.Add($"### {assembly.Name}");

            if (assembly.Files.CSharp.Count > 0)
            {
                lines.Add("C#:");
                lines.AddRange(assembly.Files.CSharp.Select(f => $"  - {f}"));
            }
            if (assembly.Files.Razor.Count > 0)
            {
                lines.Add("Razor:");
                lines.AddRange(assembly.Files.Razor.Select(f => $"  - {f}"));
            }
            if (assembly.Files.TypeScript.Count > 0)
            {
                lines.Add("TypeScript:");
                lines.AddRange(assembly.Files.TypeScript.Select(f => $"  - {f}"));
            }
            lines.Add("");
        }

        return string.Join("\n", lines);
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
            (string? validatedContent, float validatedConfidence, int rounds, List<GeneratedFile>? validatedFiles) =
                await ValidateResponseAsync(result, query, involvedAgents);

            result.CleanContent = validatedContent;
            finalConfidence = validatedConfidence;
            validationRounds = rounds;

            // Replace generated files with validated versions
            allFiles.Clear();
            allFiles.AddRange(validatedFiles);
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

        string agentsSummary = _registry.GetAgentsSummary();
        AgentExecutionResult result = await _agentExecutor.ExecuteAsync(agent, instruction, agentsSummary);

        while (result.HasPendingRequests)
        {
            StringBuilder additionalContext = new();

            if (result.FilePathsRequests.Count > 0)
            {
                foreach (string assembly in result.FilePathsRequests)
                {
                    List<string> files;
                    if (string.IsNullOrEmpty(assembly))
                    {
                        files = _fileAccess.ListFiles();
                        additionalContext.AppendLine("=== Project Files ===");
                    }
                    else
                    {
                        files = _fileAccess.ListFilesInFolder(assembly);
                        additionalContext.AppendLine($"=== Files in {assembly} ===");
                    }

                    foreach (string file in files)
                    {
                        additionalContext.AppendLine($"  {file}");
                    }
                    additionalContext.AppendLine();
                }
            }

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

                additionalContext.AppendLine($"=== Response from {targetAgent.Name} ({targetAgent.Expertise}) ===");
                additionalContext.AppendLine(subResult.CleanContent);
                additionalContext.AppendLine();
            }

            if (additionalContext.Length > 0)
            {
                string continuation = $"""
                    # Additional Information Requested
                    
                    {additionalContext}
                    
                    ---
                    
                    Continue your response incorporating this new information.
                    Remember to include [CONFIDENCE: X.X] at the end.
                    """;
                agentsSummary = _registry.GetAgentsSummary();
                result = await _agentExecutor.ExecuteAsync(agent, continuation, agentsSummary);
            }
            else
            {
                break;
            }
        }

        return result;
    }

    private async Task<(string Content, float Confidence, int Rounds, List<GeneratedFile> Files)> ValidateResponseAsync(
    AgentExecutionResult originalResult,
    string originalQuery,
    List<string> involvedAgents)
    {
        ValidationOrchestrator validationOrch = new(
            _agentFactory,
            _agentExecutor,
            _registry,
            _logger,
            _options);

        ValidationResult validationResult = await validationOrch.ValidateAndImproveAsync(
            originalResult,
            originalQuery,
            involvedAgents);

        return (
            validationResult.FinalContent,
            validationResult.FinalConfidence,
            validationResult.Iterations,
            validationResult.GeneratedFiles
        );
    }

    private async Task<RoutingDecision> DecideRoutingAsync(string query)
    {
        string existingAgents = _registry.GetAgentsSummary();
        string fileList = BuildFileListCompact();

        string routingPrompt = $$"""
        # Routing Request
        
        ## User Query
        {{query}}
        
        ## Project Files Available
        {{fileList}}
        
        ## Available Agents
        {{existingAgents}}
        
        ## Instructions
        Analyze the query and decide the best routing strategy.
        
        You MUST respond with ONLY a valid JSON object (no markdown, no explanation):
        
        {"strategy": "new", "targetExpertise": "documentation and code examples", "suggestedFiles": ["Program.cs", "README.md"], "reasoning": "Need to analyze code to generate docs"}
        
        Rules:
        - strategy: "existing" (use existing agent), "new" (create specialist), "direct" (simple query)
        - targetExpertise: NEVER empty, describe the expertise needed (e.g., "C# code documentation", "API analysis")
        - suggestedFiles: list of relevant files for context
        - reasoning: brief explanation
        """;

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = routingPrompt
        });

        _logger.LogOrchestratorInteraction(InteractionDirection.Input, routingPrompt);

        RoutingDecision? decision = await _aiClient.SendAsync<RoutingDecision>(
            _orchestratorHistory, maxTokens: 500);

        // Validar y corregir decision
        if (decision == null || string.IsNullOrWhiteSpace(decision.TargetExpertise))
        {
            _logger.Warning("Invalid routing decision, using fallback based on query analysis");
            decision = CreateFallbackDecision(query);
        }

        string responseJson = System.Text.Json.JsonSerializer.Serialize(decision);
        _logger.LogOrchestratorInteraction(InteractionDirection.Output, responseJson);

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = responseJson
        });

        return decision;
    }

    private RoutingDecision CreateFallbackDecision(string query)
    {
        string lowerQuery = query.ToLowerInvariant();

        string expertise;
        List<string> files;

        if (lowerQuery.Contains("document") || lowerQuery.Contains("readme") || lowerQuery.Contains("doc"))
        {
            expertise = "code documentation and examples";
            files = ["Program.cs", "README.md"];
        }
        else if (lowerQuery.Contains("test") || lowerQuery.Contains("unit"))
        {
            expertise = "unit testing and test coverage";
            files = [];
        }
        else if (lowerQuery.Contains("refactor") || lowerQuery.Contains("clean"))
        {
            expertise = "code refactoring and best practices";
            files = [];
        }
        else if (lowerQuery.Contains("bug") || lowerQuery.Contains("error") || lowerQuery.Contains("fix"))
        {
            expertise = "debugging and error analysis";
            files = [];
        }
        else
        {
            expertise = "general code analysis";
            files = ["Program.cs"];
        }

        return new RoutingDecision
        {
            Strategy = "new",
            TargetExpertise = expertise,
            SuggestedFiles = files,
            Reasoning = $"Fallback decision based on query keywords"
        };
    }

    private string BuildFileListCompact()
    {
        if (_preAnalysis == null) return "No files available";

        List<string> files = _preAnalysis.Structure.Assemblies
            .Where(a => !a.IsTestProject)
            .SelectMany(a => a.Files.CSharp.Concat(a.Files.Razor))
            .Take(20)
            .ToList();

        return string.Join(", ", files);
    }

    private async Task<Agent> GetOrCreateAgentAsync(string expertise, List<string>? files = null)
    {
        if (string.IsNullOrWhiteSpace(expertise))
        {
            expertise = "general code analysis";
            _logger.Warning("Empty expertise received, using default: general code analysis");
        }

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
        string projectContext = _preAnalysis != null
            ? $"""
            ## Project Context
            - Solution: {_preAnalysis.Structure.Solution}
            - Type: {_preAnalysis.Structure.Summary.ProjectType}
            - Assemblies: {_preAnalysis.Structure.Summary.TotalAssemblies}
            - Types: {_preAnalysis.Structure.Summary.TotalTypes}
            - Files: {_preAnalysis.Structure.Summary.TotalFiles}
            """
            : "";

        return $"""
        # Task
        
        {projectContext}
        
        ## User Query
        {query}
        
        ## Routing Context
        {routing.Reasoning}
        
        ## Instructions
        Provide a comprehensive response to the user's query.
        
        - Use [REQUEST_FILE: path="..."] to see file contents you need
        - Use [REQUEST_FILE_PATHS: assembly=""] to list all project files
        - Use [QUERY_AGENT: expertise="..." question="..."] for other expertise
        - Use [GENERATE_FILE: name="..." language="..."] to create files
        - Always end with [CONFIDENCE: X.X] (0.0 to 1.0)
        
        Respond in the same language as the query.
        """;
    }

    private void InitializeOrchestratorPrompt()
    {
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.System,
            Content = """
            # You are THEON Orchestrator
            
            THEON is a multi-agent system for code analysis. You coordinate specialized agents to answer user queries about codebases.
            
            ## Your Role
            
            1. Receive user queries about a codebase
            2. Decide which specialist agent should handle each query
            3. Create new agents when needed with specific expertise
            
            ## How Routing Works
            
            When you receive a query, you must decide:
            - Can an existing agent handle this? → Use "existing"
            - Do we need a new specialist? → Use "new" 
            - Is it trivial? → Use "direct"
            
            ## Response Format
            
            You ALWAYS respond with a JSON object. Nothing else. No markdown. No explanation.
            
            Example for documentation query:
            {"strategy":"new","targetExpertise":"C# documentation generation","suggestedFiles":["Program.cs","README.md"],"reasoning":"Need specialist to analyze code and generate docs"}
            
            Example for bug fix:
            {"strategy":"new","targetExpertise":"debugging and error analysis","suggestedFiles":[],"reasoning":"Need to investigate the reported issue"}
            
            ## Rules
            
            - targetExpertise must NEVER be empty
            - targetExpertise should be specific: "Blazor component lifecycle" not "frontend"
            - suggestedFiles should list files relevant to the query
            - reasoning explains your decision briefly
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