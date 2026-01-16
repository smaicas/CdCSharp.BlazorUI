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
        _formatter = new PlainTextFormatter();
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<List<SpecialistResult>> ExecuteAllAsync(
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        _logger.Trace($"Starting specialist execution for {plan.Specialists.Count} specialists");

        List<SpecialistResult> results = [];

        IOrderedEnumerable<SpecialistTask> orderedTasks = plan.Specialists.OrderBy(s => s.Priority);

        int taskNumber = 0;
        foreach (SpecialistTask task in orderedTasks)
        {
            taskNumber++;
            _logger.Progress($"Running specialist: {task.Name} ({taskNumber}/{plan.Specialists.Count})");
            _logger.Trace($"Specialist: {task.SpecialistId}, Priority: {task.Priority}");
            _logger.Trace($"  Target sections: {string.Join(", ", task.TargetSections)}");
            _logger.Trace($"  Prompts to execute: {task.Prompts.Count}");

            List<SpecialistResult> taskResults = await ExecuteSpecialistAsync(task, plan, destructured);
            results.AddRange(taskResults);

            _logger.Trace($"Specialist {task.Name} completed: {taskResults.Count} results produced");
        }

        _logger.Trace($"All specialists completed: {results.Count} total results");

        return results;
    }

    private async Task<List<SpecialistResult>> ExecuteSpecialistAsync(
        SpecialistTask task,
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        List<SpecialistResult> results = [];

        _logger.Trace($"Building context for {task.Name}...");
        string contextContent = await BuildContextAsync(task, plan, destructured);
        _logger.Trace($"Context built: {contextContent.Length} chars");

        int promptNumber = 0;
        foreach (SpecialistPrompt prompt in task.Prompts)
        {
            promptNumber++;
            _logger.Verbose($"  Executing prompt: {prompt.Id} ({promptNumber}/{task.Prompts.Count})");
            _logger.Trace($"  Prompt ID: {prompt.Id}");
            _logger.Trace($"  MaxTokens: {prompt.MaxTokens}");
            _logger.Trace($"  Instruction: {TruncateForLog(prompt.Instruction, 100)}");

            string fullPrompt = BuildFullPrompt(task, prompt, plan.CriticalContext, contextContent);
            _logger.Trace($"  Full prompt length: {fullPrompt.Length} chars (~{fullPrompt.Length / 4} tokens)");

            if (_cache != null)
            {
                (bool hit, string? cached) = _cache.TryGetQuery(fullPrompt, task.SpecialistId);
                if (hit && cached != null)
                {
                    _logger.Trace($"  Cache HIT for prompt {prompt.Id}");
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
                else
                {
                    _logger.Trace($"  Cache MISS for prompt {prompt.Id}");
                }
            }

            _logger.Trace($"  Sending to AI...");
            DateTime startTime = DateTime.UtcNow;
            string response = await _ai.SendAsync(fullPrompt, prompt.MaxTokens);
            TimeSpan elapsed = DateTime.UtcNow - startTime;
            _logger.Trace($"  AI response received in {elapsed.TotalSeconds:F2}s");

            if (!string.IsNullOrWhiteSpace(response))
            {
                _logger.Trace($"  Response length: {response.Length} chars (~{response.Length / 4} tokens)");

                _cache?.SetQuery(fullPrompt, task.SpecialistId, response);

                results.Add(new SpecialistResult
                {
                    SpecialistId = task.SpecialistId,
                    PromptId = prompt.Id,
                    Content = response,
                    TargetSections = task.TargetSections,
                    TokenCount = response.Length / 4
                });

                _logger.Trace($"  Result stored successfully");
            }
            else
            {
                _logger.Warning($"  Empty response from AI for prompt {prompt.Id}");
                _logger.Trace($"  Skipping empty result");
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
        int assemblyCount = 0;
        int fileCount = 0;

        _logger.Trace($"Building context from {task.RequiredFiles.Destructured.Count} assemblies and {task.RequiredFiles.FullContent.Count} files");

        foreach (string assemblyName in task.RequiredFiles.Destructured)
        {
            if (destructured.TryGetValue(assemblyName, out DestructuredAssembly? assembly))
            {
                string formatted = _formatter.FormatDestructured(assembly);
                sb.AppendLine(formatted);
                sb.AppendLine();
                assemblyCount++;
                _logger.Trace($"  Added assembly: {assemblyName} ({formatted.Length} chars)");
            }
            else
            {
                _logger.Trace($"  Assembly not found: {assemblyName}");
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
                    string truncated = TruncateContent(content, 8000);
                    sb.AppendLine($"=== FILE: {filePath} ===");
                    sb.AppendLine(truncated);
                    sb.AppendLine();
                    fileCount++;
                    _logger.Trace($"  Added file: {filePath} ({content.Length} chars, truncated to {truncated.Length})");
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Failed to read {filePath}: {ex.Message}");
                    _logger.Trace($"  File read error: {ex}");
                }
            }
            else
            {
                _logger.Trace($"  File not found: {filePath}");
            }
        }

        string result = sb.ToString();
        _logger.Trace($"Context complete: {assemblyCount} assemblies, {fileCount} files, total {result.Length} chars");

        return result;
    }

    private string BuildFullPrompt(
        SpecialistTask task,
        SpecialistPrompt prompt,
        string criticalContext,
        string contextContent)
    {
        _logger.Trace($"Building full prompt for {task.Name}/{prompt.Id}");

        StringBuilder sb = new();

        sb.AppendLine($"You are a {task.Name}.");
        sb.AppendLine($"Focus: {task.Focus}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(criticalContext))
        {
            sb.AppendLine("CRITICAL PROJECT CONTEXT:");
            sb.AppendLine(criticalContext);
            sb.AppendLine();
            _logger.Trace($"  Added critical context: {criticalContext.Length} chars");
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

        string result = sb.ToString();
        _logger.Trace($"Full prompt constructed: {result.Length} chars, {result.Split('\n').Length} lines");

        return result;
    }

    private static string TruncateContent(string content, int maxChars)
    {
        if (content.Length <= maxChars)
            return content;

        return content[..maxChars] + "\n// ... (truncated)";
    }

    private static string TruncateForLog(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }
}