using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using System.Text;

namespace CdCSharp.Theon.Orchestration;

public interface IOrchestrator
{
    Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default);
}

public sealed class Orchestrator : IOrchestrator
{
    private readonly TheonOptions _options;
    private readonly IProjectAnalysis _analysis;
    private readonly ILlmClient _llmClient;
    private readonly IToolParser _toolParser;
    private readonly IScopeFactory _scopeFactory;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly IResponseValidator _validator;
    private readonly IExplorationStrategies _strategies;
    private readonly IOutputPlanner _planner;

    private readonly List<LlmMessage> _conversationHistory = [];
    private int _responseCounter;
    private int _maxContextTokens;
    private const int ReservedTokensForResponse = 4000;

    public Orchestrator(
        TheonOptions options,
        IProjectAnalysis analysis,
        ILlmClient llmClient,
        IToolParser toolParser,
        IScopeFactory scopeFactory,
        IFileSystem fileSystem,
        ITheonLogger logger,
        IResponseValidator validator,
        IExplorationStrategies strategies,
        IOutputPlanner planner)
    {
        _options = options;
        _analysis = analysis;
        _llmClient = llmClient;
        _toolParser = toolParser;
        _scopeFactory = scopeFactory;
        _fileSystem = fileSystem;
        _logger = logger;
        _validator = validator;
        _strategies = strategies;
        _planner = planner;
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        if (_analysis.Project == null)
            throw new InvalidOperationException("Project not analyzed");

        await EnsureContextLimitAsync(ct);

        ScopeOverride? scopeOverride = ParseScopeOverride(userInput, out string query);

        if (_conversationHistory.Count == 0)
            InitializeConversation();

        // Get exploration strategy for this query
        ExplorationStrategy strategy = _strategies.GetStrategy(query, _analysis.Project);
        _logger.Info($"Task classified as: {strategy.Type}");

        // Get output plan
        OutputPlan outputPlan = _planner.CreatePlan(query, _analysis.Project, strategy.Type);
        if (outputPlan.HasPlan)
        {
            _logger.Info($"Output plan: {outputPlan.Files.Length} file(s)");
        }

        // Build enhanced user message with strategy and plan
        string userMessage = await BuildEnhancedUserMessage(query, scopeOverride, strategy, outputPlan, ct);

        TrimConversationIfNeeded();
        _conversationHistory.Add(LlmMessage.User(userMessage));

        List<GeneratedFile> outputFiles = [];
        List<string> modifiedProjectFiles = [];
        string responseFolder = CreateResponseFolder();

        int depth = 0;
        int totalExplorations = 0;
        float confidence = 0f;
        string finalContent = "";
        int validationFailures = 0;
        const int MaxValidationFailures = 2;

        while (depth < _options.MaxExplorationDepth)
        {
            depth++;
            _logger.Debug($"Iteration {depth}, Explorations so far: {totalExplorations}");

            LlmResponse response = await _llmClient.SendAsync(_conversationHistory, ct);
            ParseResult parsed = _toolParser.Parse(response.Content);

            _conversationHistory.Add(LlmMessage.Assistant(response.Content));

            // Count explorations
            int explorationCount = parsed.Tools.Count(IsExplorationTool);
            totalExplorations += explorationCount;

            // Validate response quality
            ValidationResult validation = _validator.Validate(
                parsed,
                totalExplorations,
                strategy.Type.ToString());

            if (!validation.IsValid && validationFailures < MaxValidationFailures)
            {
                validationFailures++;
                _logger.Warning($"Validation failed (attempt {validationFailures}): {validation.Reason}");

                // Log specific issues
                foreach (ValidationIssue? issue in validation.Issues.Where(i =>
                    i.Severity is ValidationSeverity.Critical or
                    ValidationSeverity.Error))
                {
                    _logger.Warning($"  [{issue.Severity}] {issue.Category}: {issue.Description}");
                }

                // Provide corrective feedback
                string feedback = Prompts.ValidationFeedback(
                    validation.Reason ?? "Quality standards not met",
                    validation.AdjustedConfidence);

                TrimConversationIfNeeded();
                _conversationHistory.Add(LlmMessage.User(feedback));
                continue;
            }

            // Use adjusted confidence
            confidence = validation.AdjustedConfidence;
            finalContent = parsed.CleanContent;

            // Process tools
            List<ToolInvocation> explorations = parsed.Tools.Where(IsExplorationTool).ToList();
            List<ToolInvocation> outputs = parsed.Tools.Where(IsOutputTool).ToList();
            List<ToolInvocation> modifications = parsed.Tools.Where(IsModificationTool).ToList();

            // Process outputs
            foreach (ToolInvocation tool in outputs)
            {
                GeneratedFile? file = await ProcessOutputToolAsync(tool, responseFolder, outputFiles, ct);
                if (file != null)
                    outputFiles.Add(file);
            }

            // Process modifications
            foreach (ToolInvocation tool in modifications)
            {
                string? modified = await ProcessModificationToolAsync(tool, ct);
                if (modified != null)
                    modifiedProjectFiles.Add(modified);
            }

            // Check if we should continue exploring
            bool shouldContinue = explorations.Count > 0 || parsed.NeedsMoreContext;

            // Also check if we've met minimum exploration requirements
            if (!shouldContinue && totalExplorations < strategy.MinimumExplorations)
            {
                _logger.Info($"Encouraging more exploration (current: {totalExplorations}, recommended: {strategy.MinimumExplorations})");
                shouldContinue = true;

                string encouragement = Prompts.ExplorationGuidance(
                    strategy.Type.ToString(),
                    totalExplorations);

                TrimConversationIfNeeded();
                _conversationHistory.Add(LlmMessage.User(encouragement));
                continue;
            }

            if (!shouldContinue)
                break;

            // Process explorations and continue
            StringBuilder additionalContext = new();
            foreach (ToolInvocation tool in explorations)
            {
                string? context = await ProcessExplorationToolAsync(tool, ct);
                if (context != null)
                {
                    additionalContext.AppendLine(context);
                    additionalContext.AppendLine();
                }
            }

            if (additionalContext.Length > 0)
            {
                TrimConversationIfNeeded();
                _conversationHistory.Add(LlmMessage.User(
                    Prompts.ContinueWithContext(additionalContext.ToString())));
            }
            else if (parsed.NeedsMoreContext)
            {
                // LLM said it needs more context but didn't use tools
                string reminder = """
                    You indicated you need more context, but didn't use any EXPLORE tools.
                    
                    Please use specific EXPLORE tools to get the information you need:
                    - [EXPLORE_FILE: path="..."]
                    - [EXPLORE_FOLDER: path="..."]
                    - [EXPLORE_ASSEMBLY: name="..."]
                    
                    Or proceed with your response if you actually have sufficient information.
                    """;

                TrimConversationIfNeeded();
                _conversationHistory.Add(LlmMessage.User(reminder));
            }
        }

        // Self-review if confidence is below threshold
        if (confidence > 0 && confidence < _options.Exploration.LowConfidenceThreshold)
        {
            _logger.Info($"Confidence {confidence:P0} below threshold, requesting self-review...");
            _conversationHistory.Add(LlmMessage.User(Prompts.SelfReview(finalContent, confidence)));

            LlmResponse reviewResponse = await _llmClient.SendAsync(_conversationHistory, ct);
            ParseResult reviewParsed = _toolParser.Parse(reviewResponse.Content);

            // Validate the review
            ValidationResult reviewValidation = _validator.Validate(
                reviewParsed,
                totalExplorations,
                strategy.Type.ToString());

            if (reviewValidation.AdjustedConfidence > confidence)
            {
                finalContent = reviewParsed.CleanContent;
                confidence = reviewValidation.AdjustedConfidence;
                _conversationHistory.Add(LlmMessage.Assistant(reviewResponse.Content));
                _logger.Info($"Self-review improved confidence to {confidence:P0}");
            }
        }

        // Save comprehensive response
        await SaveResponseMarkdownAsync(
            responseFolder,
            query,
            finalContent,
            outputFiles,
            modifiedProjectFiles,
            confidence,
            totalExplorations,
            strategy,
            ct);

        _logger.Info($"Response saved to: {responseFolder}");
        _logger.Info($"Total explorations: {totalExplorations}, Final confidence: {confidence:P0}");

        return new OrchestratorResponse(finalContent, outputFiles, modifiedProjectFiles, confidence);
    }

    private async Task<string> BuildEnhancedUserMessage(
        string query,
        ScopeOverride? scopeOverride,
        ExplorationStrategy strategy,
        OutputPlan outputPlan,
        CancellationToken ct)
    {
        StringBuilder sb = new();

        // Add strategy guidance
        sb.AppendLine(strategy.Guidance);
        sb.AppendLine();

        // Add output plan if available
        if (outputPlan.HasPlan)
        {
            sb.AppendLine(OutputPlanner.FormatPlanForPrompt(outputPlan));
            sb.AppendLine();
        }

        // Add scope context if provided
        if (scopeOverride != null)
        {
            string? scopeContext = await BuildScopeContextAsync(scopeOverride, ct);
            if (scopeContext != null)
            {
                sb.AppendLine("# PROVIDED SCOPE CONTEXT");
                sb.AppendLine();
                sb.AppendLine(scopeContext);
                sb.AppendLine();
            }
        }

        // Add the actual query
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("# USER QUERY");
        sb.AppendLine();
        sb.AppendLine(query);

        return sb.ToString();
    }

    private async Task EnsureContextLimitAsync(CancellationToken ct)
    {
        if (_maxContextTokens == 0)
        {
            ModelInfo info = await _llmClient.GetModelInfoAsync(ct);
            _maxContextTokens = info.ContextLength - ReservedTokensForResponse;
        }
    }

    private void TrimConversationIfNeeded()
    {
        int totalTokens = _conversationHistory.Sum(m => _llmClient.EstimateTokens(m.Content));

        while (totalTokens > _maxContextTokens && _conversationHistory.Count > 3)
        {
            int removeIndex = _conversationHistory.FindIndex(m => m.Role == "user");
            if (removeIndex < 1) break;

            int nextIndex = removeIndex + 1;
            if (nextIndex < _conversationHistory.Count && _conversationHistory[nextIndex].Role == "assistant")
            {
                totalTokens -= _llmClient.EstimateTokens(_conversationHistory[nextIndex].Content);
                _conversationHistory.RemoveAt(nextIndex);
            }

            totalTokens -= _llmClient.EstimateTokens(_conversationHistory[removeIndex].Content);
            _conversationHistory.RemoveAt(removeIndex);

            _logger.Debug($"Trimmed conversation: {_conversationHistory.Count} messages, ~{totalTokens} tokens");
        }
    }

    private void InitializeConversation()
    {
        string systemPrompt = Prompts.SystemPrompt(_analysis.Project!, _options.Modification.Enabled);
        string projectOverview = Prompts.ProjectOverview(_analysis.Project!);

        _conversationHistory.Add(LlmMessage.System(systemPrompt));
        _conversationHistory.Add(LlmMessage.User($"Project structure:\n\n{projectOverview}"));
        _conversationHistory.Add(LlmMessage.Assistant("I understand the project structure and my protocols. Ready to assist with thorough code exploration and analysis."));
    }

    private record ScopeOverride(ScopeType Type, string Value);
    private enum ScopeType { File, Folder, Assembly }

    private static ScopeOverride? ParseScopeOverride(string input, out string query)
    {
        if (input.StartsWith("@file:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
            {
                query = input[(spaceIdx + 1)..];
                return new ScopeOverride(ScopeType.File, input[6..spaceIdx]);
            }
        }

        if (input.StartsWith("@folder:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
            {
                query = input[(spaceIdx + 1)..];
                return new ScopeOverride(ScopeType.Folder, input[8..spaceIdx]);
            }
        }

        if (input.StartsWith("@assembly:", StringComparison.OrdinalIgnoreCase))
        {
            int spaceIdx = input.IndexOf(' ');
            if (spaceIdx > 0)
            {
                query = input[(spaceIdx + 1)..];
                return new ScopeOverride(ScopeType.Assembly, input[10..spaceIdx]);
            }
        }

        query = input;
        return null;
    }

    private async Task<string?> BuildScopeContextAsync(ScopeOverride scope, CancellationToken ct)
    {
        return scope.Type switch
        {
            ScopeType.File => (await _scopeFactory.CreateFileScopeAsync(scope.Value, ct))?.BuildContext(),
            ScopeType.Folder => (await _scopeFactory.CreateFolderScopeAsync(scope.Value, ct))?.BuildContext(),
            ScopeType.Assembly => (await _scopeFactory.CreateAssemblyScopeAsync(scope.Value, ct))?.BuildContext(),
            _ => null
        };
    }

    private async Task<string?> ProcessExplorationToolAsync(ToolInvocation tool, CancellationToken ct)
    {
        return tool switch
        {
            ExploreAssemblyTool t => (await _scopeFactory.CreateAssemblyScopeAsync(t.Name, ct))?.BuildContext(),
            ExploreFileTool t => (await _scopeFactory.CreateFileScopeAsync(t.Path, ct))?.BuildContext(),
            ExploreFolderTool t => (await _scopeFactory.CreateFolderScopeAsync(t.Path, ct))?.BuildContext(),
            ExploreFilesTool t => (await _scopeFactory.CreateMultiFileScopeAsync(t.Paths, ct))?.BuildContext(),
            _ => null
        };
    }

    private async Task<GeneratedFile?> ProcessOutputToolAsync(
        ToolInvocation tool,
        string responseFolder,
        List<GeneratedFile> existingFiles,
        CancellationToken ct)
    {
        return tool switch
        {
            GenerateFileTool t => await HandleGenerateFileAsync(t, responseFolder, ct),
            AppendFileTool t => await HandleAppendFileAsync(t, responseFolder, existingFiles, ct),
            OverwriteFileTool t => await HandleOverwriteFileAsync(t, responseFolder, ct),
            _ => null
        };
    }

    private async Task<GeneratedFile> HandleGenerateFileAsync(GenerateFileTool tool, string folder, CancellationToken ct)
    {
        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, tool.Content, ct);
        return new GeneratedFile(tool.Name, tool.Content);
    }

    private async Task<GeneratedFile?> HandleAppendFileAsync(AppendFileTool tool, string folder, List<GeneratedFile> existing, CancellationToken ct)
    {
        GeneratedFile? existingFile = existing.FirstOrDefault(f => f.Name == tool.Name);
        string newContent = existingFile != null
            ? existingFile.Content + "\n" + tool.Content
            : tool.Content;

        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, newContent, ct);

        if (existingFile != null)
            existing.Remove(existingFile);

        return new GeneratedFile(tool.Name, newContent);
    }

    private async Task<GeneratedFile> HandleOverwriteFileAsync(OverwriteFileTool tool, string folder, CancellationToken ct)
    {
        await _fileSystem.WriteOutputFileAsync(folder, tool.Name, tool.Content, ct);
        return new GeneratedFile(tool.Name, tool.Content);
    }

    private async Task<string?> ProcessModificationToolAsync(ToolInvocation tool, CancellationToken ct)
    {
        if (tool is not ModifyProjectFileTool modifyTool)
            return null;

        if (_options.Modification.RequireConfirmation)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nModify project file: {modifyTool.Path}");
            Console.Write("Confirm? (y/N): ");
            Console.ResetColor();

            string? response = Console.ReadLine();
            if (!string.Equals(response, "y", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info($"Modification rejected: {modifyTool.Path}");
                return null;
            }
        }

        bool success = await _fileSystem.WriteProjectFileAsync(modifyTool.Path, modifyTool.Content, ct);
        if (success)
        {
            await _analysis.RefreshFileAsync(modifyTool.Path, ct);
            return modifyTool.Path;
        }

        return null;
    }

    private string CreateResponseFolder()
    {
        int number = Interlocked.Increment(ref _responseCounter);
        return $"{number:D4}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private async Task SaveResponseMarkdownAsync(
        string folder,
        string query,
        string content,
        List<GeneratedFile> files,
        List<string> modifiedFiles,
        float confidence,
        int explorationCount,
        ExplorationStrategy strategy,
        CancellationToken ct)
    {
        StringBuilder md = new();

        md.AppendLine("# Response Summary");
        md.AppendLine();
        md.AppendLine($"**Query:** {query}");
        md.AppendLine($"**Timestamp:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine($"**Task Type:** {strategy.Type}");
        md.AppendLine($"**Explorations:** {explorationCount}");
        md.AppendLine($"**Confidence:** {confidence:P0}");
        md.AppendLine();
        md.AppendLine("---");
        md.AppendLine();
        md.AppendLine("# Response");
        md.AppendLine();
        md.AppendLine(content);

        if (files.Count > 0)
        {
            md.AppendLine();
            md.AppendLine("---");
            md.AppendLine();
            md.AppendLine("## Generated Files");
            md.AppendLine();
            foreach (GeneratedFile file in files)
                md.AppendLine($"- **{file.Name}** ({file.Content.Length} chars)");
        }

        if (modifiedFiles.Count > 0)
        {
            md.AppendLine();
            md.AppendLine("## Modified Project Files");
            md.AppendLine();
            foreach (string file in modifiedFiles)
                md.AppendLine($"- {file}");
        }

        md.AppendLine();
        md.AppendLine("---");
        md.AppendLine();
        md.AppendLine("## Quality Metrics");
        md.AppendLine();
        md.AppendLine($"- **Task Type:** {strategy.Type}");
        md.AppendLine($"- **Recommended Explorations:** {strategy.MinimumExplorations}");
        md.AppendLine($"- **Actual Explorations:** {explorationCount}");
        md.AppendLine($"- **Target Confidence:** {strategy.TargetConfidence:P0}");
        md.AppendLine($"- **Actual Confidence:** {confidence:P0}");
        md.AppendLine($"- **Quality Status:** {(confidence >= strategy.TargetConfidence ? "✓ Met target" : "⚠ Below target")}");

        await _fileSystem.WriteOutputFileAsync(folder, "response.md", md.ToString(), ct);
    }

    private static bool IsExplorationTool(ToolInvocation tool) =>
        tool is ExploreAssemblyTool or ExploreFileTool or ExploreFolderTool or ExploreFilesTool;

    private static bool IsOutputTool(ToolInvocation tool) =>
        tool is GenerateFileTool or AppendFileTool or OverwriteFileTool;

    private static bool IsModificationTool(ToolInvocation tool) =>
        tool is ModifyProjectFileTool;
}