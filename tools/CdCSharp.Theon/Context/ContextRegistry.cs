using System.Text;

namespace CdCSharp.Theon.Context;

public sealed class ContextRegistry
{
    private readonly Dictionary<string, ContextMetadata> _contexts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _loadedFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public void RegisterContext(string contextName, string contextType, string speciality, int maxBudget)
    {
        lock (_lock)
        {
            _contexts[contextName] = new ContextMetadata
            {
                Name = contextName,
                ContextType = contextType,
                Speciality = speciality,
                MaxBudget = maxBudget,
                IsClone = contextName.Contains('#')
            };

            if (!_loadedFiles.ContainsKey(contextName))
                _loadedFiles[contextName] = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void RegisterLoadedFile(string contextName, string path, string content)
    {
        lock (_lock)
        {
            if (!_loadedFiles.ContainsKey(contextName))
                _loadedFiles[contextName] = new(StringComparer.OrdinalIgnoreCase);

            _loadedFiles[contextName][path] = content;
        }
    }

    public string? PeekFile(string path, string? preferredSource = null)
    {
        lock (_lock)
        {
            if (!string.IsNullOrEmpty(preferredSource))
            {
                if (_loadedFiles.TryGetValue(preferredSource, out Dictionary<string, string>? files) &&
                    files.TryGetValue(path, out string? content))
                {
                    return content;
                }
            }

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in _loadedFiles)
            {
                if (kvp.Value.TryGetValue(path, out string? content))
                {
                    return content;
                }
            }

            return null;
        }
    }

    public string? FindFileOwner(string path)
    {
        lock (_lock)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in _loadedFiles)
            {
                if (kvp.Value.ContainsKey(path))
                    return kvp.Key;
            }
            return null;
        }
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> GetAllLoadedFiles()
    {
        lock (_lock)
        {
            return _loadedFiles.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value.Keys.ToList(),
                StringComparer.OrdinalIgnoreCase);
        }
    }

    public ContextMetadata? GetContext(string contextName)
    {
        lock (_lock)
        {
            return _contexts.GetValueOrDefault(contextName);
        }
    }

    public IReadOnlyList<ContextMetadata> GetAllContexts()
    {
        lock (_lock)
        {
            return _contexts.Values.ToList();
        }
    }

    public int GetCloneCount(string baseContextType)
    {
        lock (_lock)
        {
            return _contexts.Values.Count(c => c.ContextType == baseContextType && c.IsClone);
        }
    }

    public string GenerateCloneName(string baseContextType)
    {
        lock (_lock)
        {
            int count = GetCloneCount(baseContextType) + 1;
            return $"{baseContextType}#{count}";
        }
    }

    public string GetContextsOverview(string? excludeContext = null)
    {
        lock (_lock)
        {
            StringBuilder sb = new();
            sb.AppendLine("## Active Contexts and Loaded Files");
            sb.AppendLine();

            foreach (ContextMetadata ctx in _contexts.Values.OrderBy(c => c.Name))
            {
                if (ctx.Name == excludeContext) continue;

                string cloneIndicator = ctx.IsClone ? " (clone)" : "";
                sb.AppendLine($"**{ctx.Name}**{cloneIndicator} - {ctx.Speciality}");
                sb.AppendLine($"  Budget: {ctx.MaxBudget:N0} tokens");

                if (_loadedFiles.TryGetValue(ctx.Name, out Dictionary<string, string>? files) && files.Count > 0)
                {
                    sb.AppendLine($"  Loaded files ({files.Count}):");
                    foreach (string file in files.Keys.Take(10))
                    {
                        sb.AppendLine($"    - `{file}`");
                    }
                    if (files.Count > 10)
                    {
                        sb.AppendLine($"    ... and {files.Count - 10} more");
                    }
                }
                else
                {
                    sb.AppendLine("  No files loaded");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _contexts.Clear();
            _loadedFiles.Clear();
        }
    }
}

public sealed record ContextMetadata
{
    public required string Name { get; init; }
    public required string ContextType { get; init; }
    public required string Speciality { get; init; }
    public required int MaxBudget { get; init; }
    public required bool IsClone { get; init; }
}