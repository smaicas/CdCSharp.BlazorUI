using CdCSharp.DocGen.Core.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Infrastructure;

// ============================================================================
// LOGGING SYSTEM
// ============================================================================

public interface ILogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Verbose(string message);
}

public class ConsoleLogger : ILogger
{
    private readonly bool _verbose;
    private readonly Action<string> _writer;

    public ConsoleLogger(bool verbose = false, Action<string>? writer = null)
    {
        _verbose = verbose;
        _writer = writer ?? Console.WriteLine;
    }

    public void Info(string message) => _writer(message);
    public void Warning(string message) => _writer($"⚠️  {message}");
    public void Error(string message) => _writer($"❌ {message}");
    public void Verbose(string message) { if (_verbose) _writer($"   {message}"); }
}

public class NullLogger : ILogger
{
    public void Info(string message) { }
    public void Warning(string message) { }
    public void Error(string message) { }
    public void Verbose(string message) { }
}

// ============================================================================
// AI CLIENT
// ============================================================================

public interface IAiClient : IDisposable
{
    Task<string> SummarizeAsync(string code, int maxTokens = 100);
    Task<string> ExplainPatternAsync(string patternName, List<string> files);
    Task<string> GenerateExampleAsync(TypeInfo typeInfo);
}

public class GrokClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private const string BaseUrl = "https://api.x.ai/v1";

    public GrokClient(string apiKey, ILogger? logger = null)
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _logger = logger ?? new NullLogger();
    }

    public async Task<string> SummarizeAsync(string code, int maxTokens = 100)
    {
        if (code.Length < 500) return string.Empty;

        return await SendAsync(new GrokRequest
        {
            Messages =
            [
                new("system", "You are a technical documentation expert. Summarize code implementations concisely."),
                new("user", $"Summarize this C# implementation in 1-2 sentences:\n\n{Truncate(code, 2000)}")
            ],
            Model = "grok-beta",
            MaxTokens = maxTokens,
            Temperature = 0.3
        });
    }

    public async Task<string> ExplainPatternAsync(string patternName, List<string> files)
    {
        string fileList = string.Join("\n", files.Take(10).Select(f => $"- {f}"));

        return await SendAsync(new GrokRequest
        {
            Messages =
            [
                new("system", "Explain architectural patterns concisely for technical documentation."),
                new("user", $"Pattern: {patternName}\nFiles:\n{fileList}\n\nExplain in 2-3 sentences.")
            ],
            Model = "grok-beta",
            MaxTokens = 150,
            Temperature = 0.3
        });
    }

    public async Task<string> GenerateExampleAsync(TypeInfo typeInfo)
    {
        if (typeInfo.Importance != ImportanceLevel.Critical) return string.Empty;

        string members = string.Join("\n", typeInfo.PublicMembers.Take(5).Select(m => $"- {m.Signature}"));

        return await SendAsync(new GrokRequest
        {
            Messages =
            [
                new("system", "Generate concise C# usage examples."),
                new("user", $"Type: {typeInfo.Kind} {typeInfo.Name}\nMembers:\n{members}\n\nGenerate minimal example (3-5 lines).")
            ],
            Model = "grok-beta",
            MaxTokens = 200,
            Temperature = 0.5
        });
    }

    private async Task<string> SendAsync(GrokRequest request)
    {
        try
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync("/chat/completions", request);
            response.EnsureSuccessStatusCode();
            GrokResponse? result = await response.Content.ReadFromJsonAsync<GrokResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Grok API failed: {ex.Message}");
            return string.Empty;
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "\n// ... (truncated)";

    public void Dispose() => _http?.Dispose();

    private record GrokRequest
    {
        [JsonPropertyName("messages")] public required GrokMessage[] Messages { get; init; }
        [JsonPropertyName("model")] public required string Model { get; init; }
        [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
        [JsonPropertyName("temperature")] public double Temperature { get; init; }
    }

    private record GrokMessage([property: JsonPropertyName("role")] string Role, [property: JsonPropertyName("content")] string Content);
    private record GrokResponse([property: JsonPropertyName("choices")] List<GrokChoice>? Choices);
    private record GrokChoice([property: JsonPropertyName("message")] GrokMessage? Message);
}

public class GitignorePatternMatcher
{
    private readonly List<Rule> _rules = [];
    private readonly string _basePath;

    private record Rule(
        string Original,
        Regex Regex,
        bool IsNegation,
        bool DirOnly,
        bool Anchored,
        int Priority
    );

    public GitignorePatternMatcher(string basePath)
    {
        _basePath = NormalizePath(basePath);
    }

    public void AddPattern(string pattern, int linePriority = 0)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern.StartsWith('#'))
            return;

        pattern = pattern.Trim();

        bool negate = pattern.StartsWith('!');
        if (negate)
            pattern = pattern[1..].TrimStart();

        if (string.IsNullOrEmpty(pattern))
            return;

        bool dirOnly = pattern.EndsWith('/');
        if (dirOnly)
            pattern = pattern[..^1];

        bool anchored = pattern.StartsWith('/');
        if (anchored)
            pattern = pattern[1..];

        Regex? regex = ConvertToRegex(pattern, anchored);
        if (regex == null)
            return;

        _rules.Add(new Rule(
            Original: pattern,
            Regex: regex,
            IsNegation: negate,
            DirOnly: dirOnly,
            Anchored: anchored,
            Priority: linePriority
        ));
    }

    public void AddPatterns(IEnumerable<string> patterns)
    {
        int priority = 0;
        foreach (string p in patterns)
            AddPattern(p, priority++);
    }

    public bool IsIgnored(string path)
    {
        string relative = GetRelativePath(path);
        if (string.IsNullOrEmpty(relative))
            return false;

        bool ignored = false;

        foreach (Rule rule in _rules.OrderBy(r => r.Priority))
        {
            if (Matches(relative, rule))
                ignored = !rule.IsNegation;
        }

        return ignored;
    }

    private bool Matches(string path, Rule rule)
    {
        // 1. Match directo
        if (rule.Regex.IsMatch(path))
            return true;

        // 2. Patrones no anclados pueden hacer match en cualquier nivel
        if (!rule.Anchored)
        {
            string[] segments = path.Split('/');

            for (int i = 1; i < segments.Length; i++)
            {
                string subPath = string.Join("/", segments.Skip(i));
                if (rule.Regex.IsMatch(subPath))
                    return true;
            }
        }

        // 3. Regla de directorio: si un padre coincide, todo cuelga de él
        if (rule.DirOnly)
        {
            string[] segments = path.Split('/');

            for (int i = 0; i < segments.Length; i++)
            {
                string prefix = string.Join("/", segments.Take(i + 1));
                if (rule.Regex.IsMatch(prefix))
                    return true;
            }
        }

        return false;
    }

    private static Regex? ConvertToRegex(string pattern, bool anchored)
    {
        StringBuilder sb = new();

        if (anchored)
            sb.Append("^");
        else
            sb.Append("(?:^|/)");

        for (int i = 0; i < pattern.Length; i++)
        {
            char c = pattern[i];

            if (c == '*' && i + 1 < pattern.Length && pattern[i + 1] == '*')
            {
                sb.Append(".*");
                i++;
            }
            else if (c == '*')
            {
                sb.Append("[^/]*");
            }
            else if (c == '?')
            {
                sb.Append("[^/]");
            }
            else if ("\\.[]{}()+-^$|".Contains(c))
            {
                sb.Append('\\').Append(c);
            }
            else
            {
                sb.Append(c);
            }
        }

        sb.Append("$");

        try
        {
            return new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        catch
        {
            return null;
        }
    }

    private string GetRelativePath(string fullPath)
    {
        string normalized = NormalizePath(fullPath);

        if (normalized.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            return normalized[_basePath.Length..].TrimStart('/');
        }

        return normalized;
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimEnd('/');

    public string DebugMatch(string path)
    {
        string relative = GetRelativePath(path);
        StringBuilder sb = new();

        sb.AppendLine($"Path: {relative}");
        sb.AppendLine("Rules:");

        bool ignored = false;

        foreach (Rule rule in _rules.OrderBy(r => r.Priority))
        {
            if (Matches(relative, rule))
            {
                string action = rule.IsNegation ? "INCLUDE" : "IGNORE";
                sb.AppendLine($"  [{rule.Priority}] {action}: {rule.Original}");
                ignored = !rule.IsNegation;
            }
        }

        sb.AppendLine($"Final: {(ignored ? "IGNORED" : "INCLUDED")}");
        return sb.ToString();
    }
}

/// <summary>
/// Carga archivos de ignore y configura el matcher
/// </summary>
public static class IgnoreFileLoader
{
    private static readonly string[] DefaultPatterns =
    [
        // Build outputs
        "bin/",
        "obj/",
        "out/",
        
        // IDE
        ".vs/",
        ".vscode/",
        ".idea/",
        "*.suo",
        "*.user",
        "*.userosscache",
        
        // Dependencies
        "node_modules/",
        "packages/",
        
        // OS
        ".DS_Store",
        "Thumbs.db",
        
        // Test results
        "TestResults/",
        "*.trx"
    ];

    public static async Task<(GitignorePatternMatcher Matcher, string Source)> LoadAsync(
        string projectPath,
        string? customFile,
        bool useGitignore,
        ILogger? logger = null)
    {
        logger ??= new NullLogger();

        GitignorePatternMatcher matcher = new(projectPath);
        List<string> sources = [];

        // 1. Cargar patrones por defecto primero
        matcher.AddPatterns(DefaultPatterns);
        sources.Add($"defaults ({DefaultPatterns.Length})");
        logger.Verbose($"Loaded {DefaultPatterns.Length} default patterns");

        // 2. Luego .gitignore si existe y está habilitado
        string gitignore = Path.Combine(projectPath, ".gitignore");
        if (useGitignore && File.Exists(gitignore))
        {
            try
            {
                string[] lines = await File.ReadAllLinesAsync(gitignore);
                int count = AddPatterns(matcher, lines, logger, ".gitignore");
                if (count > 0)
                    sources.Add($".gitignore ({count})");
            }
            catch (Exception ex)
            {
                logger.Warning($"Failed to load .gitignore: {ex.Message}");
            }
        }

        // 3. Luego dcignore.txt (sobrescribe .gitignore)
        string dcignore = Path.Combine(projectPath, "dcignore.txt");
        if (File.Exists(dcignore))
        {
            try
            {
                string[] lines = await File.ReadAllLinesAsync(dcignore);
                int count = AddPatterns(matcher, lines, logger, "dcignore.txt");
                if (count > 0)
                    sources.Add($"dcignore.txt ({count})");
            }
            catch (Exception ex)
            {
                logger.Warning($"Failed to load dcignore.txt: {ex.Message}");
            }
        }

        // 4. Finalmente archivo custom (máxima prioridad)
        if (!string.IsNullOrWhiteSpace(customFile))
        {
            string path = Path.IsPathRooted(customFile)
                ? customFile
                : Path.Combine(projectPath, customFile);

            if (File.Exists(path))
            {
                try
                {
                    string[] lines = await File.ReadAllLinesAsync(path);
                    int count = AddPatterns(matcher, lines, logger, Path.GetFileName(customFile));
                    if (count > 0)
                        sources.Add($"custom ({count})");
                }
                catch (Exception ex)
                {
                    logger.Warning($"Failed to load custom ignore file: {ex.Message}");
                }
            }
            else
            {
                logger.Warning($"Custom ignore file not found: {path}");
            }
        }

        string source = sources.Count > 0 ? string.Join(" + ", sources) : "none";
        return (matcher, source);
    }

    private static int AddPatterns(
        GitignorePatternMatcher matcher,
        string[] lines,
        ILogger logger,
        string sourceName)
    {
        int count = 0;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            // Ignorar líneas vacías y comentarios
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                continue;

            matcher.AddPattern(trimmed);
            count++;
        }

        if (count > 0)
            logger.Verbose($"Loaded {count} patterns from {sourceName}");

        return count;
    }
}