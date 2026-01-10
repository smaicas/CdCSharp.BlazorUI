namespace CdCSharp.BlazorUI.Core.Components.Tree;

/// <summary>
/// Cache for storing lazily-loaded tree node children.
/// Prevents redundant network calls when expanding/collapsing nodes.
/// </summary>
public sealed class TreeNodeCache<TItem>
{
    private readonly Dictionary<string, CachedEntry> _cache = [];

    /// <summary>
    /// Attempts to retrieve cached children for a node.
    /// </summary>
    /// <param name="key">The node key</param>
    /// <param name="children">The cached children if found</param>
    /// <returns>True if cache hit, false if cache miss</returns>
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

    /// <summary>
    /// Stores children for a node in the cache.
    /// </summary>
    /// <param name="key">The node key</param>
    /// <param name="children">The children to cache</param>
    public void Set(string key, IEnumerable<TItem> children)
    {
        _cache[key] = new CachedEntry(children.ToList());
    }

    /// <summary>
    /// Removes a specific node's cached children.
    /// </summary>
    /// <param name="key">The node key to invalidate</param>
    public void Invalidate(string key)
    {
        _cache.Remove(key);
    }

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    public void InvalidateAll()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the number of cached entries.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Checks if a specific key exists in the cache.
    /// </summary>
    public bool Contains(string key) => _cache.ContainsKey(key);

    private sealed class CachedEntry
    {
        public IReadOnlyList<TItem> Children { get; }

        public CachedEntry(IReadOnlyList<TItem> children)
        {
            Children = children;
        }
    }
}