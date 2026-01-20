namespace CdCSharp.Theon.Core;

public sealed class ContextBudgetManager
{
    private readonly Dictionary<string, BudgetAllocation> _allocations = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public BudgetAllocation AllocateBudget(string contextName, int maxTokens)
    {
        lock (_lock)
        {
            BudgetAllocation allocation = new(contextName, maxTokens);
            _allocations[contextName] = allocation;
            return allocation;
        }
    }

    public void RecordUsage(string contextName, int tokens)
    {
        lock (_lock)
        {
            if (_allocations.TryGetValue(contextName, out BudgetAllocation? allocation))
            {
                allocation.RecordUsage(tokens);
            }
        }
    }

    public BudgetStatus GetStatus(string contextName)
    {
        lock (_lock)
        {
            return _allocations.TryGetValue(contextName, out BudgetAllocation? allocation)
                ? allocation.Status
                : BudgetStatus.NotAllocated;
        }
    }

    public BudgetAllocation? GetAllocation(string contextName)
    {
        lock (_lock)
        {
            return _allocations.GetValueOrDefault(contextName);
        }
    }

    public IReadOnlyList<BudgetAllocation> GetAllAllocations()
    {
        lock (_lock)
        {
            return _allocations.Values.ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _allocations.Clear();
        }
    }
}

public sealed class BudgetAllocation
{
    private int _usedTokens;
    private readonly object _lock = new();

    public string ContextName { get; }
    public int MaxTokens { get; }

    public int UsedTokens
    {
        get { lock (_lock) return _usedTokens; }
    }

    public float UtilizationPercent => MaxTokens > 0 ? (float)UsedTokens / MaxTokens * 100 : 0;

    public BudgetStatus Status => UtilizationPercent switch
    {
        < 70 => BudgetStatus.Available,
        < 90 => BudgetStatus.Warning,
        _ => BudgetStatus.Exhausted
    };

    public int AvailableTokens => Math.Max(0, MaxTokens - UsedTokens);

    public BudgetAllocation(string contextName, int maxTokens)
    {
        ContextName = contextName;
        MaxTokens = maxTokens;
        _usedTokens = 0;
    }

    public bool CanAllocate(int tokens)
    {
        lock (_lock)
        {
            return _usedTokens + tokens <= MaxTokens;
        }
    }

    public void RecordUsage(int tokens)
    {
        lock (_lock)
        {
            if (!CanAllocate(tokens))
            {
                throw new BudgetExhaustedException(ContextName, MaxTokens, _usedTokens, tokens);
            }
            _usedTokens += tokens;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _usedTokens = 0;
        }
    }
}

public enum BudgetStatus
{
    NotAllocated,
    Available,
    Warning,
    Exhausted
}

public sealed class BudgetExhaustedException : Exception
{
    public string ContextName { get; }
    public int MaxTokens { get; }
    public int UsedTokens { get; }
    public int RequestedTokens { get; }

    public BudgetExhaustedException(string contextName, int maxTokens, int usedTokens, int requestedTokens)
        : base($"Budget exhausted in context '{contextName}'. Max: {maxTokens}, Used: {usedTokens}, Requested: {requestedTokens}")
    {
        ContextName = contextName;
        MaxTokens = maxTokens;
        UsedTokens = usedTokens;
        RequestedTokens = requestedTokens;
    }
}