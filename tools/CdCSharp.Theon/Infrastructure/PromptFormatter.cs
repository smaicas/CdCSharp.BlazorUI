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
        sb.AppendLine("## ⚠️ ACTIVE EXECUTION PLAN - FOLLOW STEPS IN ORDER ⚠️");
        sb.AppendLine($"**Task Types**: {string.Join(", ", plan.TaskTypes)}");
        sb.AppendLine($"**Reasoning**: {plan.Reasoning}");
        sb.AppendLine();

        // Find next pending step
        PlanStep? nextStep = plan.Steps
            .Where(s => s.Status == PlanStepStatus.Pending)
            .OrderBy(s => s.Order)
            .FirstOrDefault();

        if (nextStep != null)
        {
            sb.AppendLine("### 🎯 NEXT ACTION REQUIRED:");
            sb.AppendLine($"**Step {nextStep.Order}/{plan.Steps.Count}**: Query [{nextStep.TargetContext}]");
            sb.AppendLine($"**Question**: {nextStep.Question}");
            sb.AppendLine($"**Purpose**: {nextStep.Purpose}");
            if (nextStep.SuggestedFiles.Count > 0)
            {
                sb.AppendLine($"**Files to examine**: {string.Join(", ", nextStep.SuggestedFiles.Take(5))}");
                if (nextStep.SuggestedFiles.Count > 5)
                    sb.AppendLine($"   ... and {nextStep.SuggestedFiles.Count - 5} more");
            }
            sb.AppendLine();
            sb.AppendLine("**YOU MUST** call query_context with:");
            sb.AppendLine($"- context_name: \"{nextStep.TargetContext}\"");
            sb.AppendLine($"- question: \"{nextStep.Question}\"");
            if (nextStep.SuggestedFiles.Count > 0)
                sb.AppendLine($"- files: \"{string.Join(",", nextStep.SuggestedFiles)}\"");
            sb.AppendLine();
        }

        sb.AppendLine("### All Steps:");

        foreach (PlanStep step in plan.Steps.OrderBy(s => s.Order))
        {
            string status = step.Status switch
            {
                PlanStepStatus.Completed => "✓",
                PlanStepStatus.InProgress => "→",
                PlanStepStatus.Failed => "✗",
                _ => "○"
            };

            string highlight = step.Order == nextStep?.Order ? " ← DO THIS NOW" : "";

            // Show progress for multi-call steps
            string callProgress = step.AllowMultipleCalls
                ? $" ({step.CallCount}/{step.MaxCalls} calls)"
                : "";

            sb.AppendLine($"{status} **Step {step.Order}/{plan.Steps.Count}**: [{step.TargetContext}] {step.Purpose}{callProgress}{highlight}");
            sb.AppendLine($"   Question: {step.Question}");

            if (step.SuggestedFiles.Count > 0)
            {
                sb.AppendLine($"   Files: {string.Join(", ", step.SuggestedFiles.Take(3))}");
                if (step.SuggestedFiles.Count > 3)
                    sb.AppendLine($"   ... and {step.SuggestedFiles.Count - 3} more");
            }

            if (step.Results.Count > 0)
            {
                for (int i = 0; i < step.Results.Count; i++)
                {
                    string preview = step.Results[i].Length > 100
                        ? step.Results[i][..100] + "..."
                        : step.Results[i];
                    sb.AppendLine($"   ✓ Call {i + 1}: {preview}");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine("### Expected Outputs:");
        foreach (ExpectedOutput output in plan.ExpectedOutputs)
        {
            sb.AppendLine($"- [{output.Type}] {output.TaskType}: {output.Description}");
        }

        int completed = plan.Steps.Count(s => s.Status == PlanStepStatus.Completed);
        sb.AppendLine();
        sb.AppendLine($"**Progress**: {completed}/{plan.Steps.Count} steps completed");
        if (completed == plan.Steps.Count)
        {
            sb.AppendLine();
            sb.AppendLine("✅ ALL STEPS COMPLETE - You may now use generate_output_file");
        }

        return sb.ToString();
    }

    public string FormatError(string message)
    {
        return JsonSerializer.Serialize(new { error = message });
    }
}