using System.Collections.Concurrent;
using System.Reflection;

namespace CdCSharp.BlazorUI.Docs.Components;

public sealed class DocDemoContext
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> _cache = new();
    private static readonly IReadOnlyDictionary<string, string> _empty
        = new Dictionary<string, string>(StringComparer.Ordinal);

    private readonly IReadOnlyDictionary<string, string> _codes;

    public DocDemoContext(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        _codes = _cache.GetOrAdd(pageType, Load);
    }

    public string? Get(string key) => _codes.TryGetValue(key, out string? v) ? v : null;

    private static IReadOnlyDictionary<string, string> Load(Type t)
    {
        FieldInfo? f = t.GetField(
            "__DocDemoCodes",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        return f?.GetValue(null) as IReadOnlyDictionary<string, string> ?? _empty;
    }
}
