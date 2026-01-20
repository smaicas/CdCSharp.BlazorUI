using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;
using System.Text;
using System.Text.Json;

namespace CdCSharp.Theon.Infrastructure;

public sealed class PromptFormatter
{
    private readonly SharedProjectKnowledge _knowledge;
    private readonly ContextRegistry _registry;
    private readonly ContextBudgetManager _budgetManager;

    public PromptFormatter(
        SharedProjectKnowledge knowledge,
        ContextRegistry registry,
        ContextBudgetManager budgetManager)
    {
        _knowledge = knowledge;
        _registry = registry;
        _budgetManager = budgetManager;
    }

    public string FormatFileIndex()
    {
        StringBuilder sb = new();
        sb.AppendLine("## File Index (use exact paths with tools)");
        sb.AppendLine();

        IOrderedEnumerable<IGrouping<string, KeyValuePair<string, FileSummary>>> groupedByDirectory = _knowledge.FileIndex
            .Where(kvp => kvp.Value.EstimatedTokens > 0)
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key) ?? "")
            .OrderBy(g => g.Key);

        foreach (IGrouping<string, KeyValuePair<string, FileSummary>>? group in groupedByDirectory)
        {
            string dirName = string.IsNullOrEmpty(group.Key) ? "(root)" : group.Key;
            sb.AppendLine($"**{dirName}/**");

            foreach (KeyValuePair<string, FileSummary> file in group.OrderBy(f => f.Key))
            {
                sb.AppendLine($"  - `{file.Key}` ({file.Value.EstimatedTokens} tokens)");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string FormatContextsOverview(string? excludeContext = null)
    {
        return _registry.GetContextsOverview(excludeContext);
    }

    public string FormatContextStatus(
        string name,
        string contextType,
        int budgetUsed,
        int budgetMax,
        int cloneDepth,
        int maxCloneDepth)
    {
        int budgetPercent = budgetMax > 0 ? (budgetUsed * 100 / budgetMax) : 0;

        BudgetAllocation? allocation = _budgetManager.GetAllocation(name);
        string budgetStatus = allocation?.Status.ToString() ?? "Unknown";

        return $"""
            ## Your Status
            Context: {name} ({contextType})
            Budget: {budgetUsed:N0} / {budgetMax:N0} tokens ({budgetPercent}% used) - {budgetStatus}
            Clone Depth: {cloneDepth} / {maxCloneDepth}
            """;
    }

    public string FormatLoadedFiles(IReadOnlyDictionary<string, string> fileContents)
    {
        if (fileContents.Count == 0)
            return "No files loaded permanently yet. Use `read_file` to load files into your context.";

        StringBuilder sb = new();
        sb.AppendLine("These files are PERMANENTLY loaded in your context:");
        sb.AppendLine();

        foreach (KeyValuePair<string, string> kvp in fileContents)
        {
            sb.AppendLine($"### {kvp.Key}");
            sb.AppendLine("```csharp");
            sb.AppendLine(kvp.Value);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string FormatPeekedFiles(IReadOnlyDictionary<string, string> peekedFiles)
    {
        if (peekedFiles.Count == 0)
            return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine("## Peeked Files (EPHEMERAL - only for this response)");
        sb.AppendLine();
        sb.AppendLine("These files were peeked and are available ONLY for generating this response.");
        sb.AppendLine("They will NOT be available in future responses unless you peek them again.");
        sb.AppendLine();

        foreach (KeyValuePair<string, string> kvp in peekedFiles)
        {
            sb.AppendLine($"### {kvp.Key} (ephemeral)");
            sb.AppendLine("```csharp");
            sb.AppendLine(kvp.Value);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public string FormatPendingChanges(IEnumerable<(string Id, string Path, string Description)> changes)
    {
        List<(string Id, string Path, string Description)> list = changes.ToList();
        if (list.Count == 0)
            return string.Empty;

        return string.Join("\n", list.Select(c => $"- [{c.Id}] {c.Path}: {c.Description}"));
    }

    public string FormatPlanStatus(ExecutionPlan? plan)
    {
        if (plan == null)
            return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine("## Current Execution Plan");
        sb.AppendLine($"**Task Types**: {string.Join(", ", plan.TaskTypes)}");
        sb.AppendLine($"**Reasoning**: {plan.Reasoning}");
        sb.AppendLine();
        sb.AppendLine("### Steps:");

        foreach (PlanStep? step in plan.Steps.OrderBy(s => s.Order))
        {
            string status = step.Status switch
            {
                PlanStepStatus.Completed => "✓",
                PlanStepStatus.InProgress => "→",
                PlanStepStatus.Failed => "✗",
                _ => "○"
            };

            sb.AppendLine($"{status} **Step {step.Order}**: [{step.TargetContext}] {step.Purpose}");
            sb.AppendLine($"   Question: {step.Question}");

            if (step.SuggestedFiles.Count > 0)
            {
                sb.AppendLine($"   Files: {string.Join(", ", step.SuggestedFiles.Take(5))}");
                if (step.SuggestedFiles.Count > 5)
                    sb.AppendLine($"   ... and {step.SuggestedFiles.Count - 5} more");
            }

            if (step.Status == PlanStepStatus.Completed && step.Result != null)
            {
                string preview = step.Result.Length > 200
                    ? step.Result[..200] + "..."
                    : step.Result;
                sb.AppendLine($"   Result: {preview}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("### Expected Outputs:");
        foreach (ExpectedOutput output in plan.ExpectedOutputs)
        {
            sb.AppendLine($"- [{output.Type}] {output.TaskType}: {output.Description}");
        }

        return sb.ToString();
    }

    public string FormatError(string message)
    {
        return JsonSerializer.Serialize(new { error = message });
    }
}