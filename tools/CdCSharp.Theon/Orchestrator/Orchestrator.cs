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

        // CRÍTICO: Pasar la estructura al FileAccessTool
        _fileAccess.SetPreAnalysis(preAnalysis);

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

AVAILABLE ASSEMBLIES (for REQUEST_FILE_PATHS)
{BuildAssemblyList(preAnalysis)}

FILES (sample)
{fileList}

Confirm understanding.
"""
        });

        _orchestratorHistory.Add(new ConversationMessage
        {
            Role = MessageRole.Assistant,
            Content = $$"""{"understood":true,"project":"{{preAnalysis.Structure.Solution}}","assemblies":{{preAnalysis.Structure.Assemblies.Count}},"ready":true}"""
        });
    }

    private static string BuildAssemblyList(PreAnalysisResult preAnalysis)
    {
        List<string> lines = [];

        foreach (AssemblyStructure assembly in preAnalysis.Structure.Assemblies.Where(a => !a.IsTestProject))
        {
            int totalFiles = assembly.Files.CSharp.Count +
                            assembly.Files.Razor.Count +
                            assembly.Files.TypeScript.Count +
                            assembly.Files.Css.Count;

            lines.Add($"  - {assembly.Name} ({totalFiles} files)");
        }

        return string.Join("\n", lines);
    }

    public async Task<ResponseOutput> ProcessQueryAsync(string query)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // ✅ CAMBIO: Usar HashSet<string> para IDs únicos
        HashSet<string> involvedAgentIds = [];
        List<GeneratedFile> allFiles = [];
        List<AgentInteraction> interactions = [];

        _logger.Info($"Processing query: {query}");

        RoutingDecision routing = await DecideRoutingAsync(query);
        _logger.Debug($"Routing decision: {routing.Strategy} -> {routing.TargetExpertise}");

        Agent agent = await GetOrCreateAgentAsync(routing.TargetExpertise, routing.SuggestedFiles);

        // ✅ Agregar ID, no nombre
        involvedAgentIds.Add(agent.Id);

        // ✅ Registrar interacción inicial
        interactions.Add(new AgentInteraction
        {
            FromAgentId = null,
            ToAgentId = agent.Id,
            Type = InteractionType.Query,
            Summary = $"Route to {agent.Expertise}",
            Timestamp = DateTime.UtcNow
        });

        string instruction = BuildAgentInstruction(query, routing);
        AgentExecutionResult result = await ExecuteWithRequestsAsync(
            agent,
            instruction,
            involvedAgentIds,  // ✅ Pasar HashSet
            interactions);      // ✅ Pasar lista de interacciones

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
                involvedAgentIds,  // ✅ Pasar HashSet
                interactions);      // ✅ Pasar interacciones

            result.CleanContent = validation.FinalContent;
            result.Confidence = validation.FinalConfidence;

            allFiles.Clear();
            allFiles.AddRange(validation.GeneratedFiles);
        }

        stopwatch.Stop();

        // ✅ Convertir IDs a nombres para metadata
        List<string> involvedAgentNames = involvedAgentIds
            .Select(id => _registry.Get(id))
            .Where(a => a != null)
            .Select(a => a!.Name)
            .Distinct()
            .ToList();

        ResponseMetadata metadata = new()
        {
            AgentsInvolved = involvedAgentNames,
            ValidationRounds = validation.Iterations,
            FinalConfidence = result.Confidence,
            ProcessingTime = stopwatch.Elapsed
        };

        return await _fileOutput.SaveResponseAsync(
            query,
            result.CleanContent,
            allFiles,
            metadata,
            interactions);  // ✅ Pasar interacciones para diagrama
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
    HashSet<string> involvedAgentIds,
    List<AgentInteraction> interactions)
    {
        ValidationSummary summary = new()
        {
            FinalContent = originalResult.CleanContent,
            FinalConfidence = originalResult.Confidence,
            GeneratedFiles = [.. originalResult.GeneratedFiles],
            Iterations = 0
        };

        _logger.Info($"Starting validation (confidence: {originalResult.Confidence:P0}, threshold: {_options.Validation.ConfidenceThreshold:P0})");

        string validatorExpertise = DetermineValidatorExpertise(originalExpertise, originalResult.SuggestedValidators);

        for (int iteration = 1; iteration <= _options.Validation.MaxIterations; iteration++)
        {
            summary.Iterations = iteration;
            _logger.Info($"Validation iteration {iteration}/{_options.Validation.MaxIterations}");

            Agent validator = await GetOrCreateAgentAsync($"{validatorExpertise} validation", []);

            // ✅ Agregar validator ID
            involvedAgentIds.Add(validator.Id);

            // ✅ Registrar validación
            interactions.Add(new AgentInteraction
            {
                FromAgentId = originalAgent.Id,
                ToAgentId = validator.Id,
                Type = InteractionType.Validation,
                Summary = $"Request validation (iter {iteration})",
                Timestamp = DateTime.UtcNow
            });

            string validationPrompt = BuildValidationPrompt(
                originalQuery,
                summary.FinalContent,
                summary.GeneratedFiles,
                summary.FinalConfidence);

            AgentExecutionResult validationResult = await ExecuteWithRequestsAsync(
                validator,
                validationPrompt,
                involvedAgentIds,
                interactions);

            _logger.Debug($"Validator response confidence: {validationResult.Confidence:P0}");

            if (validationResult.Confidence >= _options.Validation.ConfidenceThreshold)
            {
                _logger.Info($"Validation approved at iteration {iteration}");

                summary.FinalContent = validationResult.CleanContent;
                summary.FinalConfidence = validationResult.Confidence;
                MergeGeneratedFiles(summary.GeneratedFiles, validationResult.GeneratedFiles);

                return summary;
            }

            string correctionPrompt = BuildCorrectionPrompt(
                originalQuery,
                summary.FinalContent,
                validationResult.CleanContent,
                validationResult.Confidence);

            _logger.Debug($"Requesting correction from original agent: {originalAgent.Name}");

            AgentExecutionResult correctionResult = await ExecuteWithRequestsAsync(
                originalAgent,
                correctionPrompt,
                involvedAgentIds,
                interactions);

            summary.FinalContent = correctionResult.CleanContent;
            summary.FinalConfidence = correctionResult.Confidence;
            MergeGeneratedFiles(summary.GeneratedFiles, correctionResult.GeneratedFiles);

            _logger.Debug($"Correction confidence: {correctionResult.Confidence:P0}");

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
    HashSet<string> involvedAgentIds,
    List<AgentInteraction> interactions,
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

        List<GeneratedFile> allGeneratedFiles = [.. result.GeneratedFiles];

        while (result.HasPendingRequests)
        {
            StringBuilder additionalContext = new();

            // File path requests
            if (result.FilePathsRequests.Count > 0)
            {
                foreach (string assemblyNameOrEmpty in result.FilePathsRequests)
                {
                    List<string> files;
                    string header;

                    if (string.IsNullOrWhiteSpace(assemblyNameOrEmpty))
                    {
                        files = _fileAccess.ListAllProjectFiles();
                        header = "PROJECT FILES (all non-test assemblies)";
                        _logger.Debug($"Agent requested all project files: {files.Count} found");
                    }
                    else
                    {
                        files = _fileAccess.ListFilesByAssembly(assemblyNameOrEmpty);
                        header = $"FILES IN ASSEMBLY '{assemblyNameOrEmpty}'";
                        _logger.Debug($"Agent requested files for assembly '{assemblyNameOrEmpty}': {files.Count} found");
                    }

                    if (files.Count == 0)
                    {
                        additionalContext.AppendLine(header);
                        additionalContext.AppendLine("  (No files found)");

                        if (!string.IsNullOrWhiteSpace(assemblyNameOrEmpty))
                        {
                            if (_preAnalysis != null)
                            {
                                List<string> available = _preAnalysis.Structure.Assemblies
                                    .Where(a => !a.IsTestProject)
                                    .Select(a => a.Name)
                                    .ToList();

                                additionalContext.AppendLine($"  Available assemblies: {string.Join(", ", available)}");
                            }
                        }
                    }
                    else
                    {
                        additionalContext.AppendLine(header);

                        IEnumerable<IGrouping<string, string>> byExtension = files.GroupBy(f => Path.GetExtension(f).ToLowerInvariant());

                        foreach (IGrouping<string, string>? group in byExtension.OrderBy(g => g.Key))
                        {
                            string ext = group.Key;
                            string label = ext switch
                            {
                                ".cs" => "C# files",
                                ".razor" => "Razor files",
                                ".ts" => "TypeScript files",
                                ".tsx" => "TypeScript JSX files",
                                ".css" => "CSS files",
                                ".scss" => "SCSS files",
                                _ => $"{ext} files"
                            };

                            additionalContext.AppendLine($"  {label}:");
                            foreach (string file in group.Take(100))
                            {
                                additionalContext.AppendLine($"    {file}");
                            }

                            if (group.Count() > 100)
                            {
                                additionalContext.AppendLine($"    ... and {group.Count() - 100} more");
                            }
                        }
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

                // ✅ Agregar ID al HashSet (automáticamente evita duplicados)
                involvedAgentIds.Add(subAgent.Id);

                // ✅ Registrar interacción
                interactions.Add(new AgentInteraction
                {
                    FromAgentId = agent.Id,
                    ToAgentId = subAgent.Id,
                    Type = InteractionType.Creation,
                    Summary = $"Created {subAgent.Name}",
                    Timestamp = DateTime.UtcNow
                });

                additionalContext.AppendLine($"AGENT CREATED: {subAgent.Name} ({subAgent.Id})");
                additionalContext.AppendLine($"  Expertise: {subAgent.Expertise}");
                additionalContext.AppendLine();
            }

            // Agent queries
            foreach (AgentRequest queryReq in result.AgentQueries)
            {
                // ✅ ACTUALIZADO: Buscar por ID directamente
                _logger.Debug($"Cross-agent query to: {queryReq.TargetAgentId}");

                Agent? targetAgent = _registry.Get(queryReq.TargetAgentId!);

                if (targetAgent == null)
                {
                    // ✅ CRÍTICO: Si el agente no existe, registrar error
                    _logger.Warning($"Agent not found: {queryReq.TargetAgentId}");

                    additionalContext.AppendLine($"ERROR: Agent '{queryReq.TargetAgentId}' not found.");
                    additionalContext.AppendLine("Available agents:");

                    foreach (Agent availableAgent in _registry.AllAgents)
                    {
                        additionalContext.AppendLine($"  - {availableAgent.Id}: {availableAgent.Name} ({availableAgent.Expertise})");
                    }
                    additionalContext.AppendLine();

                    continue; // Saltar esta query
                }

                if (targetAgent.State == AgentState.Sleeping)
                {
                    _agentFactory.Wake(targetAgent);
                }

                // Agregar ID al HashSet
                involvedAgentIds.Add(targetAgent.Id);

                // Registrar query
                interactions.Add(new AgentInteraction
                {
                    FromAgentId = agent.Id,
                    ToAgentId = targetAgent.Id,
                    Type = InteractionType.Query,
                    Summary = queryReq.Payload.Length > 50
                        ? queryReq.Payload[..50] + "..."
                        : queryReq.Payload,
                    Timestamp = DateTime.UtcNow
                });

                AgentExecutionResult subResult = await ExecuteWithRequestsAsync(
                    targetAgent,
                    queryReq.Payload,
                    involvedAgentIds,
                    interactions,
                    depth + 1);

                // Registrar respuesta
                interactions.Add(new AgentInteraction
                {
                    FromAgentId = targetAgent.Id,
                    ToAgentId = agent.Id,
                    Type = InteractionType.Response,
                    Summary = $"Confidence: {subResult.Confidence:P0}",
                    Timestamp = DateTime.UtcNow
                });

                additionalContext.AppendLine($"RESPONSE FROM {targetAgent.Name} (ID: {targetAgent.Id})");
                additionalContext.AppendLine(subResult.CleanContent);
                additionalContext.AppendLine($"CONFIDENCE: {subResult.Confidence:F2}");
                additionalContext.AppendLine();

                if (subResult.GeneratedFiles.Count > 0)
                {
                    _logger.Debug($"Sub-query generated {subResult.GeneratedFiles.Count} files");
                    allGeneratedFiles.AddRange(subResult.GeneratedFiles);
                }
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

                if (result.GeneratedFiles.Count > 0)
                {
                    allGeneratedFiles.AddRange(result.GeneratedFiles);
                }
            }
            else
            {
                break;
            }
        }

        result.GeneratedFiles.Clear();
        result.GeneratedFiles.AddRange(allGeneratedFiles);

        _logger.Debug($"ExecuteWithRequestsAsync returning {result.GeneratedFiles.Count} total files");

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