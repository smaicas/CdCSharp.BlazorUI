using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Abstractions.Orchestration;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace CdCSharp.DocGen.Core.Orchestration;

public class SpecialistRunner : ISpecialistRunner
{
    private readonly IConversationManager _conversationManager;
    private readonly ISpecialistRegistry _registry;
    private readonly ICacheManager _cache;
    private readonly IPlainTextFormatter _formatter;
    private readonly ILogger<SpecialistRunner> _logger;
    private readonly string _projectRoot;

    public SpecialistRunner(
        IConversationManager conversationManager,
        ISpecialistRegistry registry,
        ICacheManager cache,
        IPlainTextFormatter formatter,
        IOptions<DocGenOptions> options,
        ILogger<SpecialistRunner> logger)
    {
        _conversationManager = conversationManager;
        _registry = registry;
        _cache = cache;
        _formatter = formatter;
        _projectRoot = options.Value.ProjectPath;
        _logger = logger;
    }

    public async Task<List<SpecialistResult>> ExecuteAllAsync(
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _logger.LogDebug("Starting specialist execution for {Count} specialists", plan.Specialists.Count);

        List<SpecialistResult> results = [];

        IOrderedEnumerable<SpecialistTask> orderedTasks = plan.Specialists.OrderBy(s => s.Priority);

        int taskNumber = 0;
        foreach (SpecialistTask task in orderedTasks)
        {
            taskNumber++;
            _logger.LogInformation("Running specialist: {Name} ({Current}/{Total})",
                task.Name, taskNumber, plan.Specialists.Count);

            List<SpecialistResult> taskResults = await ExecuteSpecialistAsync(task, plan, destructured);
            results.AddRange(taskResults);

            _logger.LogDebug("Specialist {Name} completed: {Count} results produced", task.Name, taskResults.Count);
        }

        _conversationManager.ClearAll();

        _logger.LogDebug("All specialists completed: {Count} total results", results.Count);

        return results;
    }

    private async Task<List<SpecialistResult>> ExecuteSpecialistAsync(
        SpecialistTask task,
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        List<SpecialistResult> results = [];

        SpecialistDefinition? definition = _registry.Get(task.SpecialistId);

        if (definition == null)
        {
            _logger.LogWarning("Specialist definition not found for ID: {Id}", task.SpecialistId);
            return results;
        }

        IConversation conversation = _conversationManager.CreateConversation(
            task.SpecialistId,
            definition.SystemPrompt);

        _logger.LogDebug("Building context for {Name}...", task.Name);
        string contextContent = await BuildContextAsync(task, plan.CriticalContext, destructured);
        conversation.AddContext(contextContent);
        _logger.LogDebug("Context built: {Length} chars", contextContent.Length);

        int promptNumber = 0;
        foreach (SpecialistPrompt prompt in task.Prompts)
        {
            promptNumber++;
            _logger.LogDebug("Executing prompt: {Id} ({Current}/{Total})",
                prompt.Id, promptNumber, task.Prompts.Count);

            string cacheKey = BuildCacheKey(task.SpecialistId, prompt.Id, contextContent);
            (bool hit, string? cached) = _cache.TryGetQuery(cacheKey, task.SpecialistId);

            if (hit && cached != null)
            {
                _logger.LogDebug("Cache HIT for prompt {Id}", prompt.Id);
                results.Add(CreateResult(task, prompt, cached));
                continue;
            }

            string userMessage = BuildUserMessage(prompt);

            _logger.LogDebug("Sending to conversation...");
            DateTime startTime = DateTime.UtcNow;
            string response = await conversation.SendAsync(userMessage, prompt.MaxTokens);
            TimeSpan elapsed = DateTime.UtcNow - startTime;

            _logger.LogDebug("Response received in {Elapsed:F2}s", elapsed.TotalSeconds);

            if (!string.IsNullOrWhiteSpace(response))
            {
                _cache.SetQuery(cacheKey, task.SpecialistId, response);
                results.Add(CreateResult(task, prompt, response));
                _logger.LogDebug("Result stored successfully");
            }
            else
            {
                _logger.LogWarning("Empty response for prompt {Id}", prompt.Id);
            }
        }

        return results;
    }

    private async Task<string> BuildContextAsync(
        SpecialistTask task,
        string criticalContext,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(criticalContext))
        {
            sb.AppendLine("PROJECT CONTEXT:");
            sb.AppendLine(criticalContext);
            sb.AppendLine();
        }

        sb.AppendLine("SPECIALIST FOCUS:");
        sb.AppendLine(task.Focus);
        sb.AppendLine();

        sb.AppendLine("RELEVANT PROJECT INFORMATION:");
        sb.AppendLine();

        foreach (string assemblyName in task.Prompts.SelectMany(p => p.RequiredFiles.Destructured))
        {
            if (destructured.TryGetValue(assemblyName, out DestructuredAssembly? assembly))
            {
                sb.AppendLine(_formatter.FormatDestructured(assembly));
                sb.AppendLine();
            }
        }

        foreach (string filePath in task.Prompts.SelectMany(p => p.RequiredFiles.FullContent))
        {
            string fullPath = Path.Combine(_projectRoot, filePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(fullPath);
                    string truncated = TruncateContent(content, 8000);
                    sb.AppendLine($"=== FILE: {filePath} ===");
                    sb.AppendLine(truncated);
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read {Path}", filePath);
                }
            }
        }

        return sb.ToString();
    }

    private static string BuildUserMessage(SpecialistPrompt prompt)
    {
        return $"""
            TASK:
            {prompt.Instruction}
            
            EXPECTED OUTPUT:
            {prompt.ExpectedOutput}
            """;
    }

    private static string BuildCacheKey(string specialistId, string promptId, string context)
    {
        int contextHash = context.GetHashCode();
        return $"{specialistId}:{promptId}:{contextHash}";
    }

    private static SpecialistResult CreateResult(SpecialistTask task, SpecialistPrompt prompt, string content)
    {
        return new SpecialistResult
        {
            SpecialistId = task.SpecialistId,
            PromptId = prompt.Id,
            Content = content,
            TargetSections = task.TargetSections,
            TokenCount = content.Length / 4
        };
    }

    private static string TruncateContent(string content, int maxChars)
    {
        if (content.Length <= maxChars)
            return content;

        return content[..maxChars] + "\n// ... (truncated)";
    }
}