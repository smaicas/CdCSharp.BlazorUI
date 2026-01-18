using CdCSharp.Theon.AI;

public sealed record ContextQuery
{
    public required string Question { get; init; }
    public IReadOnlyList<string>? InitialFiles { get; init; }
    public IReadOnlyList<string>? InitialPatterns { get; init; }

    public static ContextQuery Simple(string question) => new() { Question = question };

    public static ContextQuery WithFiles(string question, params string[] files) =>
        new() { Question = question, InitialFiles = files };

    public static ContextQuery WithPatterns(string question, params string[] patterns) =>
        new() { Question = question, InitialPatterns = patterns };
}

public sealed class ContextState
{
    public List<Message> History { get; } = [];
    public HashSet<string> LoadedFiles { get; } = [];
    public Dictionary<string, string> FileContents { get; } = [];
    public Dictionary<string, object> ToolResults { get; } = [];
    public int EstimatedTokens { get; private set; }

    public void AddMessage(Message message)
    {
        History.Add(message);
        EstimatedTokens += EstimateTokens(message.Content ?? string.Empty);
    }

    public void AddFileContent(string path, string content)
    {
        if (LoadedFiles.Add(path))
        {
            FileContents[path] = content;
            EstimatedTokens += EstimateTokens(content);
        }
    }

    public void CacheToolResult(string toolCallId, object result)
    {
        ToolResults[toolCallId] = result;
    }

    public bool HasCapacityFor(int additionalTokens, int maxTokenBudget)
    {
        return EstimatedTokens + additionalTokens <= maxTokenBudget;
    }

    public void Clear()
    {
        History.Clear();
        LoadedFiles.Clear();
        FileContents.Clear();
        ToolResults.Clear();
        EstimatedTokens = 0;
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)(text.Length / 3.5);
    }
}

public sealed record ContextConfiguration
{
    public required string Name { get; init; }
    public required string SystemPrompt { get; init; }
    public bool IsStateful { get; init; } = false;
    public int MaxTokenBudget { get; init; } = 8000;
    public bool CanReadFiles { get; init; } = true;
    public bool CanSearchFiles { get; init; } = true;
    public bool CanListAssemblies { get; init; } = true;

    public static ContextConfiguration Stateless(string name, string systemPrompt) =>
        new()
        {
            Name = name,
            SystemPrompt = systemPrompt,
            IsStateful = false
        };

    public static ContextConfiguration Stateful(string name, string systemPrompt) =>
        new()
        {
            Name = name,
            SystemPrompt = systemPrompt,
            IsStateful = true
        };
}