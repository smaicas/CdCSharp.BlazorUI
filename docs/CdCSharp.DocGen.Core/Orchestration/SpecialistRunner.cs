using CdCSharp.DocGen.Core.Cache;
using CdCSharp.DocGen.Core.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Orchestration;

public class SpecialistRunner
{
    private readonly IAiClient _ai;
    private readonly CacheManager? _cache;
    private readonly IProjectFormatter _formatter;
    private readonly ILogger _logger;
    private readonly string _projectRoot;

    public SpecialistRunner(
        IAiClient ai,
        string projectRoot,
        CacheManager? cache = null,
        ILogger? logger = null)
    {
        _ai = ai;
        _projectRoot = projectRoot;
        _cache = cache;
        _formatter = new OptimizedFormatter();
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<List<SpecialistResult>> ExecuteAllAsync(
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        List<SpecialistResult> results = [];

        IOrderedEnumerable<SpecialistTask> orderedTasks = plan.Specialists.OrderBy(s => s.Priority);

        foreach (SpecialistTask task in orderedTasks)
        {
            _logger.Progress($"Running specialist: {task.Name}");

            List<SpecialistResult> taskResults = await ExecuteSpecialistAsync(task, plan, destructured);
            results.AddRange(taskResults);
        }

        return results;
    }

    private async Task<List<SpecialistResult>> ExecuteSpecialistAsync(
        SpecialistTask task,
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        List<SpecialistResult> results = [];

        string contextContent = await BuildContextAsync(task, plan, destructured);

        foreach (SpecialistPrompt prompt in task.Prompts)
        {
            _logger.Verbose($"  Executing prompt: {prompt.Id}");

            string fullPrompt = BuildFullPrompt(task, prompt, plan.CriticalContext, contextContent);

            if (_cache != null)
            {
                (bool hit, string? cached) = _cache.TryGetQuery(fullPrompt, task.SpecialistId);
                if (hit && cached != null)
                {
                    results.Add(new SpecialistResult
                    {
                        SpecialistId = task.SpecialistId,
                        PromptId = prompt.Id,
                        Content = cached,
                        TargetSections = task.TargetSections,
                        TokenCount = cached.Length / 4
                    });
                    continue;
                }
            }

            string response = await _ai.SendAsync(fullPrompt, prompt.MaxTokens);

            if (!string.IsNullOrWhiteSpace(response))
            {
                _cache?.SetQuery(fullPrompt, task.SpecialistId, response);

                results.Add(new SpecialistResult
                {
                    SpecialistId = task.SpecialistId,
                    PromptId = prompt.Id,
                    Content = response,
                    TargetSections = task.TargetSections,
                    TokenCount = response.Length / 4
                });
            }
        }

        return results;
    }

    private async Task<string> BuildContextAsync(
        SpecialistTask task,
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        foreach (string assemblyName in task.RequiredFiles.Destructured)
        {
            if (destructured.TryGetValue(assemblyName, out DestructuredAssembly? assembly))
            {
                sb.AppendLine(_formatter.FormatDestructured(assembly));
                sb.AppendLine();
            }
        }

        foreach (string filePath in task.RequiredFiles.FullContent)
        {
            string fullPath = Path.Combine(_projectRoot, filePath);
            if (File.Exists(fullPath))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(fullPath);
                    sb.AppendLine($"=== FILE: {filePath} ===");
                    sb.AppendLine(TruncateContent(content, 8000));
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to read {filePath}: {ex.Message}");
                }
            }
        }

        return sb.ToString();
    }

    private string BuildFullPrompt(
        SpecialistTask task,
        SpecialistPrompt prompt,
        string criticalContext,
        string contextContent)
    {
        StringBuilder sb = new();

        sb.AppendLine($"You are a {task.Name}.");
        sb.AppendLine($"Focus: {task.Focus}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(criticalContext))
        {
            sb.AppendLine("CRITICAL PROJECT CONTEXT:");
            sb.AppendLine(criticalContext);
            sb.AppendLine();
        }

        sb.AppendLine("PROJECT INFORMATION:");
        sb.AppendLine(contextContent);
        sb.AppendLine();

        sb.AppendLine("TASK:");
        sb.AppendLine(prompt.Instruction);
        sb.AppendLine();

        sb.AppendLine("EXPECTED OUTPUT:");
        sb.AppendLine(prompt.ExpectedOutput);
        sb.AppendLine();

        sb.AppendLine("RULES:");
        sb.AppendLine("- Write in clear, professional technical documentation style");
        sb.AppendLine("- Use markdown formatting");
        sb.AppendLine("- Include code examples where relevant");
        sb.AppendLine("- Be concise but thorough");

        return sb.ToString();
    }

    private static string TruncateContent(string content, int maxChars)
    {
        if (content.Length <= maxChars)
            return content;

        return content[..maxChars] + "\n// ... (truncated)";
    }
}