using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tools;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace CdCSharp.Theon.Orchestration;

public interface IOrchestrator
{
    Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default);
}
public sealed partial class Orchestrator : IOrchestrator
{
    private readonly TheonOptions _options;
    private readonly IProjectAnalysis _analysis;
    private readonly ILlmClient _llmClient;
    private readonly IToolRegistry _toolRegistry;
    private readonly IScopeFactory _scopeFactory;
    private readonly IFileSystem _fileSystem;
    private readonly IOutputContext _outputContext;
    private readonly ITheonLogger _logger;
    private readonly IResponseValidator _validator;
    private readonly IExplorationStrategies _strategies;
    private readonly IOutputPlanner _planner;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<LlmMessage> _conversationHistory = [];
    private int _responseCounter;
    private int _maxContextTokens;
    private const int ReservedTokensForResponse = 4000;

    public Orchestrator(
        TheonOptions options,
        IProjectAnalysis analysis,
        ILlmClient llmClient,
        IToolRegistry toolRegistry,
        IScopeFactory scopeFactory,
        IFileSystem fileSystem,
        IOutputContext outputContext,
        ITheonLogger logger,
        IResponseValidator validator,
        IExplorationStrategies strategies,
        IOutputPlanner planner,
        IPromptBuilder promptBuilder,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _analysis = analysis;
        _llmClient = llmClient;
        _toolRegistry = toolRegistry;
        _scopeFactory = scopeFactory;
        _fileSystem = fileSystem;
        _outputContext = outputContext;
        _logger = logger;
        _validator = validator;
        _strategies = strategies;
        _planner = planner;
        _promptBuilder = promptBuilder;
        _serviceProvider = serviceProvider;
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        if (_analysis.Project == null)
            throw new InvalidOperationException("Project not analyzed");

        await EnsureContextLimitAsync(ct);

        ScopeOverride? scopeOverride = ParseScopeOverride(userInput, out string query);

        if (_conversationHistory.Count == 0)
            await InitializeConversationAsync(ct);

        ExplorationStrategy strategy = _strategies.GetStrategy(query, _analysis.Project);
        _logger.Info($"Task classified as: {strategy.Type}");

        string responseFolder = CreateResponseFolder();
        _outputContext.SetResponseFolder(responseFolder);

        OutputPlan outputPlan = _planner.CreatePlan(query, _analysis.Project, strategy.Type);

        string userMessage = await BuildEnhancedUserMessage(query, scopeOverride, strategy, outputPlan, ct);

        TrimConversationIfNeeded();
        _conversationHistory.Add(LlmMessage.User(userMessage));

        List<string> modifiedProjectFiles = [];
        int totalExplorations = 0;
        float confidence = 0f;
        string finalContent = "";

        bool useNativeTools = _llmClient.Capabilities.SupportsTools;

        for (int depth = 0; depth < _options.MaxExplorationDepth; depth++)
        {
            _logger.Debug($"Iteration {depth + 1}, Explorations: {totalExplorations}");

            LlmResponse response = await _llmClient.SendAsync(_conversationHistory, ct);

            if (useNativeTools && response.HasToolCalls)
            {
                _conversationHistory.Add(LlmMessage.AssistantWithToolCalls(response.ToolCalls!));

                ToolExecutionContext toolContext = new(query, _serviceProvider);

                foreach (LlmToolCall toolCall in response.ToolCalls!)
                {
                    _logger.Debug($"Executing tool: {toolCall.Name}");

                    if (toolCall.Name.StartsWith("EXPLORE", StringComparison.OrdinalIgnoreCase))
                        totalExplorations++;

                    ToolExecutionResult result = await _toolRegistry.ExecuteAsync(
                        toolCall.Name, toolCall.Arguments, toolContext, ct);

                    string toolResultContent = result.Success
                        ? result.ContextToAdd ?? "Tool executed successfully"
                        : $"Error: {result.ErrorMessage}";

                    _conversationHistory.Add(LlmMessage.ToolResult(toolCall.Id, toolResultContent));

                    if (toolCall.Name == "MODIFY_PROJECT_FILE" && result.Success)
                    {
                        JsonDocument args = JsonDocument.Parse(toolCall.Arguments);
                        if (args.RootElement.TryGetProperty("path", out JsonElement pathEl))
                        {
                            string? path = pathEl.GetString();
                            if (path != null) modifiedProjectFiles.Add(path);
                        }
                    }
                }

                continue;
            }

            _conversationHistory.Add(LlmMessage.Assistant(response.Content));
            finalContent = response.Content;

            if (!useNativeTools)
            {
                (int explorations, float? parsedConfidence, bool hasMoreTools) =
                    await ProcessLegacyResponseAsync(response.Content, responseFolder, modifiedProjectFiles, ct);

                totalExplorations += explorations;
                if (parsedConfidence.HasValue) confidence = parsedConfidence.Value;

                if (hasMoreTools)
                    continue;
            }

            confidence = ExtractConfidence(response.Content) ?? CalculateConfidence(totalExplorations);
            break;
        }

        List<GeneratedFile> outputFiles = _outputContext.GetAllGeneratedFiles()
            .Select(kvp => new GeneratedFile(kvp.Key, kvp.Value))
            .ToList();

        await SaveResponseMarkdownAsync(
            responseFolder, query, finalContent, outputFiles,
            modifiedProjectFiles, confidence, totalExplorations, strategy, ct);

        _logger.Info($"Response saved to: {responseFolder}");
        _logger.Info($"Explorations: {totalExplorations}, Confidence: {confidence:P0}");

        return new OrchestratorResponse(finalContent, outputFiles, modifiedProjectFiles, confidence);
    }

    private async Task<(int explorations, float? confidence, bool hasMoreTools)> ProcessLegacyResponseAsync(
        string content,
        string responseFolder,
        List<string> modifiedFiles,
        CancellationToken ct)
    {
        int explorations = 0;
        float? confidence = null;
        StringBuilder additionalContext = new();
        ToolExecutionContext toolContext = new(null, _serviceProvider);

        foreach (Match m in ExploreAssemblyRegex().Matches(content))
        {
            explorations++;
            string args = JsonSerializer.Serialize(new { name = m.Groups[1].Value });
            ToolExecutionResult result = await _toolRegistry.ExecuteAsync("EXPLORE_ASSEMBLY", args, toolContext, ct);
            if (result.Success) additionalContext.AppendLine(result.ContextToAdd);
        }

        foreach (Match m in ExploreFileRegex().Matches(content))
        {
            explorations++;
            string args = JsonSerializer.Serialize(new { path = m.Groups[1].Value });
            ToolExecutionResult result = await _toolRegistry.ExecuteAsync("EXPLORE_FILE", args, toolContext, ct);
            if (result.Success) additionalContext.AppendLine(result.ContextToAdd);
        }

        foreach (Match m in ExploreFolderRegex().Matches(content))
        {
            explorations++;
            string args = JsonSerializer.Serialize(new { path = m.Groups[1].Value });
            ToolExecutionResult result = await _toolRegistry.ExecuteAsync("EXPLORE_FOLDER", args, toolContext, ct);
            if (result.Success) additionalContext.AppendLine(result.ContextToAdd);
        }

        foreach (Match m in GenerateFileRegex().Matches(content))
        {
            string fileName = m.Groups[1].Value;
            string fileContent = m.Groups[3].Value.Trim();
            await _fileSystem.WriteOutputFileAsync(responseFolder, fileName, fileContent, ct);
            _outputContext.UpdateGeneratedFile(fileName, fileContent);
        }

        Match confMatch = ConfidenceRegex().Match(content);
        if (confMatch.Success && float.TryParse(confMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float conf))
        {
            confidence = Math.Clamp(conf, 0f, 1f);
        }

        bool hasMoreTools = additionalContext.Length > 0;

        if (hasMoreTools)
        {
            TrimConversationIfNeeded();
            _conversationHistory.Add(LlmMessage.User(
                _promptBuilder.BuildContinueWithContext(additionalContext.ToString())));
        }

        return (explorations, confidence, hasMoreTools);
    }

    private float? ExtractConfidence(string content)
    {
        Match match = ConfidenceRegex().Match(content);
        if (match.Success && float.TryParse(match.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float conf))
        {
            return Math.Clamp(conf, 0f, 1f);
        }
        return null;
    }

    private static float CalculateConfidence(int explorations) => explorations switch
    {
        0 => 0.3f,
        1 or 2 => 0.5f,
        3 or 4 or 5 => 0.7f,
        _ => 0.85f
    };

    private async Task EnsureContextLimitAsync(CancellationToken ct)
    {
        if (_maxContextTokens == 0)
        {
            ModelInfo info = await _llmClient.GetModelInfoAsync(ct);
            _maxContextTokens = info.ContextLength - ReservedTokensForResponse;
        }
    }

    private async Task InitializeConversationAsync(CancellationToken ct)
    {
        await _llmClient.DetectCapabilitiesAsync(ct);

        string systemPrompt = _promptBuilder.BuildSystemPrompt(
            _analysis.Project!,
            _options.Modification.Enabled,
            _llmClient.Capabilities.SupportsTools);

        string projectOverview = _promptBuilder.BuildProjectOverview(_analysis.Project!);

        _conversationHistory.Add(LlmMessage.System(systemPrompt));
        _conversationHistory.Add(LlmMessage.User($"Project structure:\n\n{projectOverview}"));
        _conversationHistory.Add(LlmMessage.Assistant(
            "I understand the project structure. Ready to assist with thorough code exploration."));
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
        }
    }

    private async Task<string> BuildEnhancedUserMessage(
        string query,
        ScopeOverride? scopeOverride,
        ExplorationStrategy strategy,
        OutputPlan outputPlan,
        CancellationToken ct)
    {
        StringBuilder sb = new();

        sb.AppendLine(strategy.Guidance);
        sb.AppendLine();

        if (outputPlan.HasPlan)
        {
            sb.AppendLine(OutputPlanner.FormatPlanForPrompt(outputPlan));
            sb.AppendLine();
        }

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

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("# USER QUERY");
        sb.AppendLine();
        sb.AppendLine(query);

        return sb.ToString();
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
        IContextScope? contextScope = scope.Type switch
        {
            ScopeType.File => await _scopeFactory.CreateFileScopeAsync(scope.Value, ct),
            ScopeType.Folder => await _scopeFactory.CreateFolderScopeAsync(scope.Value, ct),
            ScopeType.Assembly => await _scopeFactory.CreateAssemblyScopeAsync(scope.Value, ct),
            _ => null
        };

        return contextScope != null ? _promptBuilder.BuildScopeContext(contextScope) : null;
    }

    private string CreateResponseFolder()
    {
        int number = Interlocked.Increment(ref _responseCounter);
        return $"{number:D4}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private async Task SaveResponseMarkdownAsync(
        string folder, string query, string content, List<GeneratedFile> files,
        List<string> modifiedFiles, float confidence, int explorationCount,
        ExplorationStrategy strategy, CancellationToken ct)
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

        await _fileSystem.WriteOutputFileAsync(folder, "response.md", md.ToString(), ct);
    }

    [GeneratedRegex(@"\[EXPLORE_ASSEMBLY:\s*name=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreAssemblyRegex();

    [GeneratedRegex(@"\[EXPLORE_FILE:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreFileRegex();

    [GeneratedRegex(@"\[EXPLORE_FOLDER:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreFolderRegex();

    [GeneratedRegex(@"\[GENERATE_FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*([\s\S]*?)\[/GENERATE_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex GenerateFileRegex();

    [GeneratedRegex(@"\[CONFIDENCE:\s*([\d.]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ConfidenceRegex();
}