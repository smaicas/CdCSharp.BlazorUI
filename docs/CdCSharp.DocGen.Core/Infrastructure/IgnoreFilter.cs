using CdCSharp.DocGen.Core.Abstractions.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CdCSharp.DocGen.Core.Infrastructure;

public class IgnoreFilter : IIgnoreFilter
{
    private readonly ILogger<IgnoreFilter> _logger;
    private readonly List<IgnoreRule> _rules = [];
    private string _basePath = string.Empty;

    private static readonly string[] DefaultPatterns =
    [
        "bin/",
        "obj/",
        ".vs/",
        ".vscode/",
        ".idea/",
        ".git/",
        "node_modules/",
        "packages/",
        "TestResults/",
        "*.user",
        "*.suo",
        ".DS_Store"
    ];

    public string Source { get; private set; } = "defaults";

    public IgnoreFilter(ILogger<IgnoreFilter> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(string projectPath)
    {
        _basePath = NormalizePath(projectPath);
        _rules.Clear();

        AddPatterns(DefaultPatterns);

        string dgignorePath = Path.Combine(projectPath, "dgignore.txt");
        string gitignorePath = Path.Combine(projectPath, ".gitignore");

        if (File.Exists(dgignorePath))
        {
            string[] lines = await File.ReadAllLinesAsync(dgignorePath);
            int count = AddPatterns(lines);
            Source = $"dgignore.txt ({count} patterns)";
            _logger.LogDebug("Loaded {Count} patterns from dgignore.txt", count);
        }
        else if (File.Exists(gitignorePath))
        {
            string[] lines = await File.ReadAllLinesAsync(gitignorePath);
            int count = AddPatterns(lines);
            Source = $".gitignore ({count} patterns)";
            _logger.LogDebug("Loaded {Count} patterns from .gitignore", count);
        }

        _logger.LogDebug("Using ignore patterns: {Source}", Source);
    }

    public bool IsIgnored(string path)
    {
        string relativePath = GetRelativePath(path);
        if (string.IsNullOrEmpty(relativePath))
            return false;

        bool ignored = false;

        foreach (IgnoreRule rule in _rules)
        {
            if (rule.Matches(relativePath))
                ignored = !rule.IsNegation;
        }

        return ignored;
    }

    private int AddPatterns(IEnumerable<string> patterns)
    {
        int count = 0;
        foreach (string line in patterns)
        {
            string pattern = line.Trim();

            if (string.IsNullOrEmpty(pattern) || pattern.StartsWith('#'))
                continue;

            _rules.Add(new IgnoreRule(pattern));
            count++;
        }
        return count;
    }

    private string GetRelativePath(string fullPath)
    {
        string normalized = NormalizePath(fullPath);

        if (normalized.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            return normalized[_basePath.Length..].TrimStart('/');

        return normalized;
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimEnd('/');

    private sealed class IgnoreRule
    {
        private readonly string _pattern;
        public bool IsNegation { get; }
        public bool IsDirectoryOnly { get; }
        public bool IsAnchored { get; }

        public IgnoreRule(string pattern)
        {
            IsNegation = pattern.StartsWith('!');
            if (IsNegation)
                pattern = pattern[1..];

            IsDirectoryOnly = pattern.EndsWith('/');
            if (IsDirectoryOnly)
                pattern = pattern[..^1];

            IsAnchored = pattern.StartsWith('/') || pattern.Contains('/');
            if (pattern.StartsWith('/'))
                pattern = pattern[1..];

            _pattern = pattern;
        }

        public bool Matches(string path)
        {
            if (IsAnchored)
                return MatchesPattern(path, _pattern);

            string[] segments = path.Split('/');
            for (int i = 0; i <= segments.Length; i++)
            {
                string subPath = string.Join("/", segments.Skip(i));
                if (MatchesPattern(subPath, _pattern))
                    return true;
            }

            return false;
        }

        private bool MatchesPattern(string path, string pattern)
        {
            if (pattern.Contains('*'))
                return MatchesWildcard(path, pattern);

            if (IsDirectoryOnly)
                return path.StartsWith(pattern + "/", StringComparison.OrdinalIgnoreCase) ||
                       path.Equals(pattern, StringComparison.OrdinalIgnoreCase);

            return path.StartsWith(pattern + "/", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals(pattern, StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("/" + pattern, StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesWildcard(string path, string pattern)
        {
            if (pattern == "**")
                return true;

            if (pattern.StartsWith("**"))
            {
                string suffix = pattern[2..].TrimStart('/');
                return path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ||
                       path.Contains("/" + suffix, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.StartsWith("*."))
            {
                string extension = pattern[1..];
                return path.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.EndsWith("/*"))
            {
                string prefix = pattern[..^2];
                return path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
            }

            string regexPattern = "^" +
                pattern.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".") +
                "$";

            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(
                    path, regexPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}