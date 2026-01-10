namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeCache<TItem>
{
    private readonly Dictionary<string, IReadOnlyList<TItem>> _cache = [];

    public bool TryGet(string key, out IEnumerable<TItem>? children)
    {
        if (_cache.TryGetValue(key, out IReadOnlyList<TItem>? entry))
        {
            children = entry;
            return true;
        }
        children = null;
        return false;
    }

    public void Set(string key, IEnumerable<TItem> children)
        => _cache[key] = children.ToList();

    public void Invalidate(string key) => _cache.Remove(key);

    public void InvalidateAll() => _cache.Clear();
}