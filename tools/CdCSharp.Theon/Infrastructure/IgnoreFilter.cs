namespace CdCSharp.Theon.Infrastructure;

public class IgnoreFilter
{
    private readonly List<IgnoreRule> _rules = [];
    private string _basePath = string.Empty;

    private static readonly string[] DefaultPatterns =
    [
        "bin/", "obj/", ".vs/", ".vscode/", ".idea/", ".git/",
        "node_modules/", "packages/", "TestResults/",
        "*.user", "*.suo", ".DS_Store", "_Imports.razor",
    ];

    public async Task InitializeAsync(string projectPath)
    {
        _basePath = NormalizePath(projectPath);
        _rules.Clear();

        foreach (string pattern in DefaultPatterns)
            _rules.Add(new IgnoreRule(pattern));

        string dgignorePath = Path.Combine(projectPath, "dgignore.txt");
        string gitignorePath = Path.Combine(projectPath, ".gitignore");

        string? ignoreFile = File.Exists(dgignorePath) ? dgignorePath :
                             File.Exists(gitignorePath) ? gitignorePath : null;

        if (ignoreFile != null)
        {
            string[] lines = await File.ReadAllLinesAsync(ignoreFile);
            foreach (string line in lines)
            {
                string pattern = line.Trim();
                if (!string.IsNullOrEmpty(pattern) && !pattern.StartsWith('#'))
                    _rules.Add(new IgnoreRule(pattern));
            }
        }
    }

    public bool IsIgnored(string path)
    {
        string relativePath = GetRelativePath(path);
        if (string.IsNullOrEmpty(relativePath)) return false;

        bool ignored = false;
        foreach (IgnoreRule rule in _rules)
        {
            if (rule.Matches(relativePath))
                ignored = !rule.IsNegation;
        }
        return ignored;
    }

    private string GetRelativePath(string fullPath)
    {
        string normalized = NormalizePath(fullPath);
        return normalized.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase)
            ? normalized[_basePath.Length..].TrimStart('/')
            : normalized;
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').TrimEnd('/');

    private class IgnoreRule
    {
        private readonly string _pattern;
        public bool IsNegation { get; }
        private readonly bool _isDirectory;

        public IgnoreRule(string pattern)
        {
            IsNegation = pattern.StartsWith('!');
            if (IsNegation) pattern = pattern[1..];

            _isDirectory = pattern.EndsWith('/');
            if (_isDirectory) pattern = pattern[..^1];

            if (pattern.StartsWith('/')) pattern = pattern[1..];
            _pattern = pattern;
        }

        public bool Matches(string path)
        {
            if (_pattern.Contains('*'))
                return MatchesWildcard(path);

            return path.StartsWith(_pattern + "/", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals(_pattern, StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/" + _pattern, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesWildcard(string path)
        {
            if (_pattern.StartsWith("*."))
            {
                string ext = _pattern[1..];
                return path.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}