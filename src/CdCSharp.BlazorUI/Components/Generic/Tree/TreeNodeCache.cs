namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeCache<TItem>
{
    private readonly Dictionary<string, CachedEntry> _cache = [];

    public bool TryGet(string key, out IEnumerable<TItem>? children)
    {
        if (_cache.TryGetValue(key, out CachedEntry? entry))
        {
            children = entry.Children;
            return true;
        }
        children = null;
        return false;
    }

    public void Set(string key, IEnumerable<TItem> children)
    {
        _cache[key] = new CachedEntry(children.ToList());
    }

    public void Invalidate(string key)
    {
        _cache.Remove(key);
    }

    public void InvalidateAll()
    {
        _cache.Clear();
    }

    private sealed class CachedEntry(IReadOnlyList<TItem> children)
    {
        public IReadOnlyList<TItem> Children { get; } = children;
    }
}
