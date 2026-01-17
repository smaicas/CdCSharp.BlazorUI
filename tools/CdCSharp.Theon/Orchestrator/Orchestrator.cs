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
    private readonly GeneratedFilesTracker _filesTracker;

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
        GeneratedFilesTracker filesTracker,
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
        _filesTracker = filesTracker;
        _logger = logger;
        _options = options;

        InitializeOrchestratorPrompt();
    }

    public void SetProjectStructure(PreAnalysisResult preAnalysis)
    {
        _preAnalysis = preAnalysis;

        string projectOverview = _formatter.FormatProjectStructure(preAnalysis.Structure);
        string fileList = BuildFileListCompact(preAnalysis);

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = $"""
PROJECT LOADED

Name: {preAnalysis.Structure.Solution}
Type: {preAnalysis.Structure.Summary.ProjectType}
Assemblies: {preAnalysis.Structure.Summary.TotalAssemblies}
Types: {preAnalysis.Structure.Summary.TotalTypes}
Files: {preAnalysis.Structure.Summary.TotalFiles}

STRUCTURE
{projectOverview}

FILES
{fileList}

Confirm understanding.
"""
        });

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = $$"""{"understood":true,"project":"{{preAnalysis.Structure.Solution}}","ready":true}"""
        });
    }

    public async Task<ResponseOutput> ProcessQueryAsync(string query)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<string> involvedAgents = [];
        List<GeneratedFile> allFiles = [];

        _logger.Info($"Processing query: {query}");

        RoutingDecision routing = await DecideRoutingAsync(query);
        _logger.Debug($"Routing decision: {routing.Strategy} -> {routing.TargetExpertise}");

        Agent agent = await GetOrCreateAgentAsync(routing.TargetExpertise, routing.SuggestedFiles);
        involvedAgents.Add(agent.Name);

        string instruction = BuildAgentInstruction(query, routing);
        AgentExecutionResult result = await ExecuteWithRequestsAsync(agent, instruction, involvedAgents);

        allFiles.AddRange(result.GeneratedFiles);

        // Validación si es necesaria
        ValidationSummary validation = new() { Iterations = 0 };

        if (NeedsValidation(result))
        {
            validation = await ValidateAndImproveAsync(
                agent,
                result,
                query,
                routing.TargetExpertise,
                involvedAgents);

            result.CleanContent = validation.FinalContent;
            result.Confidence = validation.FinalConfidence;

            allFiles.Clear();
            allFiles.AddRange(validation.GeneratedFiles);
        }

        stopwatch.Stop();

        ResponseMetadata metadata = new()
        {
            AgentsInvolved = involvedAgents,
            ValidationRounds = validation.Iterations,
            FinalConfidence = result.Confidence,
            ProcessingTime = stopwatch.Elapsed
        };

        return await _fileOutput.SaveResponseAsync(query, result.CleanContent, allFiles, metadata);
    }

    private bool NeedsValidation(AgentExecutionResult result)
    {
        return result.Confidence < _options.Validation.ConfidenceThreshold
            || result.SuggestedValidators.Count > 0;
    }

    private async Task<ValidationSummary> ValidateAndImproveAsync(
        Agent originalAgent,
        AgentExecutionResult originalResult,
        string originalQuery,
        string originalExpertise,
        List<string> involvedAgents)
    {
        ValidationSummary summary = new()
        {
            FinalContent = originalResult.CleanContent,
            FinalConfidence = originalResult.Confidence,
            GeneratedFiles = [.. originalResult.GeneratedFiles],
            Iterations = 0
        };

        _logger.Info($"Starting validation (confidence: {originalResult.Confidence:P0}, threshold: {_options.Validation.ConfidenceThreshold:P0})");

        // Determinar expertise del validador basado en el tema
        string validatorExpertise = DetermineValidatorExpertise(originalExpertise, originalResult.SuggestedValidators);

        for (int iteration = 1; iteration <= _options.Validation.MaxIterations; iteration++)
        {
            summary.Iterations = iteration;
            _logger.Info($"Validation iteration {iteration}/{_options.Validation.MaxIterations}");

            // Obtener o crear agente validador
            Agent validator = await GetOrCreateAgentAsync($"{validatorExpertise} validation", []);
            if (!involvedAgents.Contains(validator.Name))
            {
                involvedAgents.Add(validator.Name);
            }

            // Validador revisa el trabajo
            string validationPrompt = BuildValidationPrompt(
                originalQuery,
                summary.FinalContent,
                summary.GeneratedFiles,
                summary.FinalConfidence);

            AgentExecutionResult validationResult = await ExecuteWithRequestsAsync(
                validator,
                validationPrompt,
                involvedAgents);

            _logger.Debug($"Validator response confidence: {validationResult.Confidence:P0}");

            // Si el validador aprueba, terminamos
            if (validationResult.Confidence >= _options.Validation.ConfidenceThreshold)
            {
                _logger.Info($"Validation approved at iteration {iteration}");

                summary.FinalContent = validationResult.CleanContent;
                summary.FinalConfidence = validationResult.Confidence;
                MergeGeneratedFiles(summary.GeneratedFiles, validationResult.GeneratedFiles);

                return summary;
            }

            // Validador tiene objeciones, agente original debe corregir
            string correctionPrompt = BuildCorrectionPrompt(
                originalQuery,
                summary.FinalContent,
                validationResult.CleanContent,
                validationResult.Confidence);

            _logger.Debug($"Requesting correction from original agent: {originalAgent.Name}");

            AgentExecutionResult correctionResult = await ExecuteWithRequestsAsync(
                originalAgent,
                correctionPrompt,
                involvedAgents);

            summary.FinalContent = correctionResult.CleanContent;
            summary.FinalConfidence = correctionResult.Confidence;
            MergeGeneratedFiles(summary.GeneratedFiles, correctionResult.GeneratedFiles);

            _logger.Debug($"Correction confidence: {correctionResult.Confidence:P0}");

            // Si después de corregir alcanza el threshold, terminamos
            if (correctionResult.Confidence >= _options.Validation.ConfidenceThreshold)
            {
                _logger.Info($"Correction approved at iteration {iteration}");
                return summary;
            }
        }

        _logger.Warning($"Validation did not reach threshold after {_options.Validation.MaxIterations} iterations");
        return summary;
    }

    private string DetermineValidatorExpertise(string originalExpertise, List<string> suggestedValidators)
    {
        if (suggestedValidators.Count > 0)
        {
            return string.Join(" and ", suggestedValidators);
        }

        // Derivar expertise de validación del tema original
        return originalExpertise switch
        {
            var e when e.Contains("security", StringComparison.OrdinalIgnoreCase) => "security review",
            var e when e.Contains("performance", StringComparison.OrdinalIgnoreCase) => "performance review",
            var e when e.Contains("blazor", StringComparison.OrdinalIgnoreCase) => "Blazor best practices",
            var e when e.Contains("api", StringComparison.OrdinalIgnoreCase) => "API design review",
            var e when e.Contains("database", StringComparison.OrdinalIgnoreCase) => "database design review",
            var e when e.Contains("test", StringComparison.OrdinalIgnoreCase) => "test coverage review",
            _ => "code quality review"
        };
    }

    private string BuildValidationPrompt(
        string originalQuery,
        string currentContent,
        List<GeneratedFile> generatedFiles,
        float currentConfidence)
    {
        StringBuilder sb = new();

        sb.AppendLine("VALIDATION TASK");
        sb.AppendLine();
        sb.AppendLine("You must review the following response and determine if it adequately addresses the query.");
        sb.AppendLine();
        sb.AppendLine("ORIGINAL QUERY");
        sb.AppendLine(originalQuery);
        sb.AppendLine();
        sb.AppendLine($"RESPONSE TO VALIDATE (current confidence: {currentConfidence:F2})");
        sb.AppendLine(currentContent);
        sb.AppendLine();

        if (generatedFiles.Count > 0)
        {
            sb.AppendLine("GENERATED FILES");
            foreach (GeneratedFile file in generatedFiles)
            {
                sb.AppendLine($"  {file.FileName} ({file.Language}, {file.Content.Length} chars)");
            }
            sb.AppendLine();
        }

        sb.AppendLine("YOUR TASK");
        sb.AppendLine("1. Identify any gaps, errors, or issues in the response.");
        sb.AppendLine("2. If the response is adequate, set high confidence.");
        sb.AppendLine("3. If there are issues, describe them clearly so the original agent can correct.");
        sb.AppendLine("4. You can use REQUEST_FILE to verify claims against actual code.");
        sb.AppendLine();
        sb.AppendLine("RESPONSE FORMAT");
        sb.AppendLine("If approved: confirm the response is adequate.");
        sb.AppendLine("If issues found: list the specific problems that need correction.");
        sb.AppendLine("End with [CONFIDENCE: X.X] where 0.7+ means approved.");

        return sb.ToString();
    }

    private string BuildCorrectionPrompt(
        string originalQuery,
        string yourPreviousResponse,
        string validatorFeedback,
        float validatorConfidence)
    {
        StringBuilder sb = new();

        sb.AppendLine("CORRECTION REQUIRED");
        sb.AppendLine();
        sb.AppendLine("A validator has reviewed your response and found issues.");
        sb.AppendLine();
        sb.AppendLine("ORIGINAL QUERY");
        sb.AppendLine(originalQuery);
        sb.AppendLine();
        sb.AppendLine("YOUR PREVIOUS RESPONSE");
        sb.AppendLine(yourPreviousResponse);
        sb.AppendLine();
        sb.AppendLine($"VALIDATOR FEEDBACK (confidence: {validatorConfidence:F2})");
        sb.AppendLine(validatorFeedback);
        sb.AppendLine();
        sb.AppendLine("YOUR TASK");
        sb.AppendLine("1. Address each issue raised by the validator.");
        sb.AppendLine("2. Provide a corrected and improved response.");
        sb.AppendLine("3. If you need additional information, use REQUEST_FILE or QUERY_AGENT.");
        sb.AppendLine("4. If you generated files, regenerate them with corrections using GENERATE_FILE.");
        sb.AppendLine();
        sb.AppendLine("End with [CONFIDENCE: X.X]");

        return sb.ToString();
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
            return new AgentExecutionResult
            {
                AgentId = agent.Id,
                CleanContent = "Max depth reached",
                Confidence = 0.1f
            };
        }

        string agentsSummary = _registry.GetAgentsSummary();
        AgentExecutionResult result = await _agentExecutor.ExecuteAsync(agent, instruction, agentsSummary);

        while (result.HasPendingRequests)
        {
            StringBuilder additionalContext = new();

            // File path requests
            if (result.FilePathsRequests.Count > 0)
            {
                foreach (string assembly in result.FilePathsRequests)
                {
                    List<string> files = string.IsNullOrEmpty(assembly)
                        ? _fileAccess.ListFiles()
                        : _fileAccess.ListFilesInFolder(assembly);

                    string header = string.IsNullOrEmpty(assembly) ? "PROJECT FILES" : $"FILES IN {assembly}";
                    additionalContext.AppendLine(header);
                    foreach (string file in files)
                    {
                        additionalContext.AppendLine($"  {file}");
                    }
                    additionalContext.AppendLine();
                }
            }

            // File content requests
            if (result.FileRequests.Count > 0)
            {
                _logger.Debug($"Agent requested {result.FileRequests.Count} files");
                Dictionary<string, string> files = await _fileAccess.GetFilesContentAsync(result.FileRequests);

                foreach ((string path, string content) in files)
                {
                    additionalContext.AppendLine($"FILE: {path}");
                    additionalContext.AppendLine(content);
                    additionalContext.AppendLine();
                }

                List<string> notFound = result.FileRequests.Except(files.Keys).ToList();
                foreach (string path in notFound)
                {
                    additionalContext.AppendLine($"FILE NOT FOUND: {path}");
                }
            }

            // Agent creation requests
            foreach (AgentCreationSpec spec in result.AgentCreationRequests)
            {
                _logger.Info($"Creating sub-agent: {spec.Name}");
                Agent subAgent = await _agentFactory.CreateAsync(spec);
                involvedAgents.Add(subAgent.Name);

                additionalContext.AppendLine($"AGENT CREATED: {subAgent.Name} ({subAgent.Id})");
                additionalContext.AppendLine($"  Expertise: {subAgent.Expertise}");
                additionalContext.AppendLine();
            }

            // Agent queries
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
                {
                    _agentFactory.Wake(targetAgent);
                }

                involvedAgents.Add(targetAgent.Name);

                AgentExecutionResult subResult = await ExecuteWithRequestsAsync(
                    targetAgent,
                    queryReq.Payload,
                    involvedAgents,
                    depth + 1);

                additionalContext.AppendLine($"RESPONSE FROM {targetAgent.Name} ({targetAgent.Expertise})");
                additionalContext.AppendLine(subResult.CleanContent);
                additionalContext.AppendLine($"CONFIDENCE: {subResult.Confidence:F2}");
                additionalContext.AppendLine();
            }

            if (additionalContext.Length > 0)
            {
                string continuation = $"""
ADDITIONAL INFORMATION

{additionalContext}

Continue your response with this information.
End with [CONFIDENCE: X.X]
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

    private async Task<RoutingDecision> DecideRoutingAsync(string query)
    {
        string existingAgents = _registry.GetAgentsSummary();
        string fileList = BuildFileListCompact();

        string routingPrompt = $$"""
ROUTING REQUEST

USER QUERY
{{query}}

PROJECT FILES
{{fileList}}

AVAILABLE AGENTS
{{existingAgents}}

Decide routing strategy. Respond with JSON only:
{"strategy":"new","targetExpertise":"specific expertise","suggestedFiles":["file1.cs"],"reasoning":"explanation"}

strategy: "existing" (use existing agent), "new" (create specialist), "direct" (trivial query)
targetExpertise: never empty, describe expertise needed
suggestedFiles: relevant files for context
reasoning: brief explanation
""";

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.User,
            Content = routingPrompt
        });

        _logger.LogOrchestratorInteraction(InteractionDirection.Input, routingPrompt);

        RoutingDecision? decision = await _aiClient.SendAsync<RoutingDecision>(
            _orchestratorHistory, maxTokens: 500);

        if (decision == null || string.IsNullOrWhiteSpace(decision.TargetExpertise))
        {
            _logger.Warning("Invalid routing decision, using fallback");
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

        string expertise = lowerQuery switch
        {
            var q when q.Contains("document") || q.Contains("readme") => "code documentation",
            var q when q.Contains("test") => "unit testing",
            var q when q.Contains("refactor") => "code refactoring",
            var q when q.Contains("bug") || q.Contains("error") => "debugging",
            var q when q.Contains("security") => "security analysis",
            var q when q.Contains("performance") => "performance optimization",
            _ => "code analysis"
        };

        return new RoutingDecision
        {
            Strategy = "new",
            TargetExpertise = expertise,
            SuggestedFiles = [],
            Reasoning = "Fallback decision based on query keywords"
        };
    }

    private async Task<Agent> GetOrCreateAgentAsync(string expertise, List<string>? files = null)
    {
        if (string.IsNullOrWhiteSpace(expertise))
        {
            expertise = "general code analysis";
            _logger.Warning("Empty expertise received, using default");
        }

        Agent? existing = _registry.FindByExpertise(expertise);

        if (existing != null)
        {
            if (existing.State == AgentState.Sleeping)
            {
                _agentFactory.Wake(existing);
            }
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
        string toolsDocs = AITools.GetToolsDocumentation();

        string projectContext = _preAnalysis != null
            ? $"""
Project: {_preAnalysis.Structure.Solution}
Type: {_preAnalysis.Structure.Summary.ProjectType}
Assemblies: {_preAnalysis.Structure.Summary.TotalAssemblies}
Types: {_preAnalysis.Structure.Summary.TotalTypes}
Files: {_preAnalysis.Structure.Summary.TotalFiles}
"""
            : "No project context available.";

        return $"""
PROJECT CONTEXT
{projectContext}

USER QUERY
{query}

ROUTING CONTEXT
{routing.Reasoning}

TOOLS
{toolsDocs}

INSTRUCTIONS
Respond to the user query.
If you need information, use the appropriate tool with exact syntax:

When you have completed the task:
  Include [TASK_COMPLETE]
  Include [CONFIDENCE: X.X] as the last line

Respond in the same language as the query.
""";
    }

    private void InitializeOrchestratorPrompt()
    {
        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.System,
            Content = """
You are the THEON Orchestrator.
You coordinate specialized agents to answer queries about codebases.

Your task: receive a query and decide routing.

Routing options:
- strategy "existing": use an existing agent that matches the required expertise
- strategy "new": create a new specialist agent
- strategy "direct": query is trivial and needs no agent

Respond with only a JSON object. No other text.

JSON format:
{"strategy":"new","targetExpertise":"specific expertise","suggestedFiles":["file1.cs"],"reasoning":"explanation"}

Field requirements:
- strategy: one of "existing", "new", "direct"
- targetExpertise: never empty, describe the expertise needed
- suggestedFiles: list of relevant files, can be empty
- reasoning: brief explanation
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

    private static void MergeGeneratedFiles(List<GeneratedFile> target, List<GeneratedFile> source)
    {
        foreach (GeneratedFile file in source)
        {
            GeneratedFile? existing = target.FirstOrDefault(f => f.FileName == file.FileName);
            if (existing != null)
            {
                target.Remove(existing);
            }
            target.Add(file);
        }
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
            Metrics = _metrics.GetSummary(),
            GeneratedFiles = _filesTracker.GetAllRecords()
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

        _sessionManager.RestoreAgents(state, _registry, _agentFactory, _filesTracker);

        _orchestratorHistory.Clear();
        _orchestratorHistory.AddRange(state.OrchestratorHistory);

        _logger.Info($"Session restored with {state.Agents.Count} agents");

        if (state.GeneratedFiles != null)
        {
            int totalFiles = state.GeneratedFiles.Values.Sum(list => list.Count);
            _logger.Info($"Restored {totalFiles} generated files");
        }

        return true;
    }
}

public record RoutingDecision
{
    public string Strategy { get; init; } = "new";
    public string TargetExpertise { get; init; } = "";
    public List<string> SuggestedFiles { get; init; } = [];
    public string Reasoning { get; init; } = "";
}

public record ValidationSummary
{
    public string FinalContent { get; set; } = "";
    public float FinalConfidence { get; set; }
    public List<GeneratedFile> GeneratedFiles { get; set; } = [];
    public int Iterations { get; set; }
}