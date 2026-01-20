using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tracing;

namespace CdCSharp.Theon.Context;

public sealed record ContextQuery
{
    public required string Question { get; init; }
    public IReadOnlyList<string>? InitialFiles { get; init; }
    public IReadOnlyList<string>? InitialPatterns { get; init; }

    public static ContextQuery Simple(string question) => new() { Question = question };

    public static ContextQuery WithFiles(string question, params string[] files) =>
        new() { Question = question, InitialFiles = files.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray() };

    public static ContextQuery WithPatterns(string question, params string[] patterns) =>
        new() { Question = question, InitialPatterns = patterns };
}

public sealed class ContextState
{
    public List<Message> History { get; } = [];
    public HashSet<string> LoadedFiles { get; } = [];
    public Dictionary<string, string> FileContents { get; } = [];
    public int EstimatedTokens { get; private set; }
    public int DelegationDepth { get; private set; }
    public Stack<string> DelegationChain { get; } = new();

    private readonly HashSet<string> _queriedContexts = [];
    private readonly Dictionary<string, int> _queryCountByContext = [];
    private const int MaxQueriesPerContext = 3;

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

    public bool HasCapacityFor(int additionalTokens, int maxBudget)
        => EstimatedTokens + additionalTokens <= maxBudget;

    public void Clear()
    {
        History.Clear();
        LoadedFiles.Clear();
        FileContents.Clear();
        EstimatedTokens = 0;
        DelegationDepth = 0;
        DelegationChain.Clear();
        _queriedContexts.Clear();
        _queryCountByContext.Clear();
    }

    public void IncrementDelegationDepth(string targetContext)
    {
        DelegationDepth++;
        DelegationChain.Push(targetContext);
    }

    public void DecrementDelegationDepth()
    {
        if (DelegationDepth > 0)
        {
            DelegationDepth--;
            if (DelegationChain.Count > 0)
                DelegationChain.Pop();
        }
    }

    public bool CanDelegateTo(string targetContext, string question)
    {
        if (DelegationChain.Contains(targetContext))
            return false;

        string queryKey = $"{targetContext}:{question.GetHashCode()}";
        if (_queriedContexts.Contains(queryKey))
            return false;

        if (_queryCountByContext.GetValueOrDefault(targetContext, 0) >= MaxQueriesPerContext)
            return false;

        return true;
    }

    public void RecordDelegation(string targetContext, string question)
    {
        string queryKey = $"{targetContext}:{question.GetHashCode()}";
        _queriedContexts.Add(queryKey);
        _queryCountByContext[targetContext] = _queryCountByContext.GetValueOrDefault(targetContext, 0) + 1;
    }

    private static int EstimateTokens(string text) => TokenEstimator.Estimate(text);

}

public sealed record ContextConfiguration
{
    public required string Name { get; init; }
    public required string SystemPrompt { get; init; }
    public string Model { get; init; } = "default";
    public string ContextType { get; init; } = "Custom";
    public string Speciality { get; init; } = "General analysis";
    public bool IsStateful { get; init; } = false;
    public int MaxTokenBudget { get; init; } = 8000;

    // File access capabilities
    public bool CanReadFiles { get; init; } = true;
    public bool CanPeekFiles { get; init; } = true;
    public bool CanSearchFiles { get; init; } = true;

    // Context management capabilities
    public bool CanDelegateToContexts { get; init; } = true;
    public bool CanSpawnClones { get; init; } = true;

    // Limits
    public int MaxDelegationDepth { get; init; } = 3;
    public int MaxCloneDepth { get; init; } = 10;
    public int MaxClonesPerType { get; init; } = 50;
}

public interface IContextScope
{
    string Name { get; }
    string ContextType { get; }
    ContextConfiguration Configuration { get; }

    Task<Result<TResponse>> QueryAsync<TResponse>(
        ContextQuery query,
        ITracerScope? parentScope,
        CancellationToken ct) where TResponse : class, new();
}

internal sealed class ContextScope : IContextScope
{
    private readonly Context _context;

    public string Name => _context.Name;
    public string ContextType => _context.Configuration.ContextType;
    public ContextConfiguration Configuration => _context.Configuration;

    private readonly IFileSystem _fileSystem;

    public ContextScope(
        ContextConfiguration config,
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        IContextFactory factory,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        PromptFormatter promptFormatter,
        ContextBudgetManager budgetManager,
        TheonOptions options,
        int cloneDepth)
    {
        _fileSystem = fileSystem;
        _context = new Context(
            config,
            aiClient,
            projectContext,
            fileSystem,
            logger,
            tracer,
            factory,
            sharedKnowledge,
            registry,
            promptFormatter,
            budgetManager,
            options,
            cloneDepth);
    }

    public async Task<Result<TResponse>> QueryAsync<TResponse>(
    ContextQuery query,
    ITracerScope? parentScope,
    CancellationToken ct) where TResponse : class, new()
    {
        // Create a PartialTracer for this context query
        using PartialTracer partialTracer = new(_fileSystem);
        partialTracer.SetUserInput($"Context: {Name} | Question: {query.Question}");

        try
        {
            TResponse result = await _context.AskAsync<TResponse>(query, parentScope, ct);
            return Result<TResponse>.Success(result);
        }
        catch (BudgetExhaustedException ex)
        {
            return Result<TResponse>.Failure(
                Error.BudgetExhausted(ex.ContextName, ex.RequestedTokens, ex.MaxTokens - ex.UsedTokens));
        }
        catch (Exception ex)
        {
            return Result<TResponse>.Failure(Error.Custom("CONTEXT_ERROR", ex.Message));
        }
    }
}