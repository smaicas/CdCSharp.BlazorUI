namespace CdCSharp.BlazorUI.Core.Components.Tree;

public sealed class TreeEngineConfiguration<TItem>
{
    // Data-bound mode
    public Func<TItem, string>? KeySelector { get; init; }
    public Func<TItem, IEnumerable<TItem>?>? ChildrenSelector { get; init; }
    public Func<TItem, bool>? HasChildrenSelector { get; init; }
    public Func<TItem, Task<IEnumerable<TItem>>>? LoadChildrenAsync { get; init; }

    // Caching
    public TreeNodeCache<TItem>? Cache { get; init; }

    // Initial state
    public HashSet<string>? InitialExpandedKeys { get; init; }
    public bool ExpandAll { get; init; }
}
