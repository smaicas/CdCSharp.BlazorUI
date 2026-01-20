using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using System.Text;

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
        sb.AppendLine("## File Index (use exact paths with read_file)");
        sb.AppendLine();

        IEnumerable<IGrouping<string, KeyValuePair<string, FileSummary>>> groupedByDirectory = _knowledge.FileIndex
            .Where(kvp => kvp.Value.EstimatedTokens > 0)
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key) ?? "")
            .OrderBy(g => g.Key);

        foreach (IGrouping<string, KeyValuePair<string, FileSummary>> group in groupedByDirectory)
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
            return "No files loaded yet.";

        StringBuilder sb = new();
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

    public string FormatPendingChanges(IEnumerable<(string Id, string Path, string Description)> changes)
    {
        List<(string Id, string Path, string Description)> list = changes.ToList();
        if (list.Count == 0)
            return string.Empty;

        return string.Join("\n", list.Select(c => $"- [{c.Id}] {c.Path}: {c.Description}"));
    }
}