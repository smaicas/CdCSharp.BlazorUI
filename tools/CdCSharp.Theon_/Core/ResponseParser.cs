using CdCSharp.Theon.Tools;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Core;

public interface IResponseParser
{
    ParsedResponse Parse(string rawResponse, bool useStructuredFormat);
}

public sealed record ParsedResponse(
    string Content,
    IReadOnlyList<ParsedToolCall> ToolCalls,
    IReadOnlyList<ParsedGeneratedFile> GeneratedFiles,
    float? Confidence,
    bool TaskComplete,
    string? NeedMoreContext);

public sealed record ParsedToolCall(string Name, JsonElement Parameters);

public sealed record ParsedGeneratedFile(string FileName, string Language, string Content);

public sealed partial class ResponseParser : IResponseParser
{
    private readonly IToolRegistry _toolRegistry;

    public ResponseParser(IToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public ParsedResponse Parse(string rawResponse, bool useStructuredFormat)
    {
        if (useStructuredFormat)
            return ParseStructured(rawResponse);

        return ParseLegacy(rawResponse);
    }

    private ParsedResponse ParseStructured(string rawResponse)
    {
        try
        {
            string json = ExtractJson(rawResponse);
            LlmStructuredResponse? structured = JsonSerializer.Deserialize<LlmStructuredResponse>(json);

            if (structured == null)
                return ParseLegacy(rawResponse);

            List<ParsedToolCall> toolCalls = structured.ToolCalls?
                .Select(tc => new ParsedToolCall(tc.Name, tc.Parameters))
                .ToList() ?? [];

            List<ParsedGeneratedFile> files = structured.GeneratedFiles?
                .Select(f => new ParsedGeneratedFile(f.FileName, f.Language, f.Content))
                .ToList() ?? [];

            return new ParsedResponse(
                Content: structured.Content,
                ToolCalls: toolCalls,
                GeneratedFiles: files,
                Confidence: structured.Confidence,
                TaskComplete: structured.TaskComplete,
                NeedMoreContext: structured.NeedMoreContext);
        }
        catch (JsonException)
        {
            return ParseLegacy(rawResponse);
        }
    }

    private ParsedResponse ParseLegacy(string rawResponse)
    {
        List<ParsedToolCall> toolCalls = [];
        List<ParsedGeneratedFile> generatedFiles = [];
        float? confidence = null;
        string? needMoreContext = null;

        // Parse exploration tools
        foreach (Match m in ExploreAssemblyRegex().Matches(rawResponse))
        {
            string json = JsonSerializer.Serialize(new { name = m.Groups[1].Value });
            toolCalls.Add(new ParsedToolCall("EXPLORE_ASSEMBLY", JsonDocument.Parse(json).RootElement));
        }

        foreach (Match m in ExploreFileRegex().Matches(rawResponse))
        {
            string json = JsonSerializer.Serialize(new { path = m.Groups[1].Value });
            toolCalls.Add(new ParsedToolCall("EXPLORE_FILE", JsonDocument.Parse(json).RootElement));
        }

        foreach (Match m in ExploreFolderRegex().Matches(rawResponse))
        {
            string json = JsonSerializer.Serialize(new { path = m.Groups[1].Value });
            toolCalls.Add(new ParsedToolCall("EXPLORE_FOLDER", JsonDocument.Parse(json).RootElement));
        }

        foreach (Match m in ExploreFilesRegex().Matches(rawResponse))
        {
            List<string> paths = m.Groups[1].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            string json = JsonSerializer.Serialize(new { paths });
            toolCalls.Add(new ParsedToolCall("EXPLORE_FILES", JsonDocument.Parse(json).RootElement));
        }

        // Parse output tools
        foreach (Match m in GenerateFileRegex().Matches(rawResponse))
        {
            generatedFiles.Add(new ParsedGeneratedFile(
                m.Groups[1].Value,
                m.Groups[2].Value,
                m.Groups[3].Value.Trim()));
        }

        foreach (Match m in AppendFileRegex().Matches(rawResponse))
        {
            string json = JsonSerializer.Serialize(new
            {
                name = m.Groups[1].Value,
                content = m.Groups[2].Value.Trim()
            });
            toolCalls.Add(new ParsedToolCall("APPEND_FILE", JsonDocument.Parse(json).RootElement));
        }

        foreach (Match m in OverwriteFileRegex().Matches(rawResponse))
        {
            generatedFiles.Add(new ParsedGeneratedFile(
                m.Groups[1].Value,
                m.Groups[2].Value,
                m.Groups[3].Value.Trim()));
        }

        // Parse modification tools
        foreach (Match m in ModifyProjectFileRegex().Matches(rawResponse))
        {
            string json = JsonSerializer.Serialize(new
            {
                path = m.Groups[1].Value,
                content = m.Groups[2].Value.Trim()
            });
            toolCalls.Add(new ParsedToolCall("MODIFY_PROJECT_FILE", JsonDocument.Parse(json).RootElement));
        }

        // Parse confidence
        Match confMatch = ConfidenceRegex().Match(rawResponse);
        if (confMatch.Success && float.TryParse(confMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float conf))
        {
            confidence = Math.Clamp(conf, 0f, 1f);
        }

        // Parse need more context
        Match needMoreMatch = NeedMoreContextRegex().Match(rawResponse);
        if (needMoreMatch.Success)
            needMoreContext = needMoreMatch.Groups[1].Value;

        string cleanContent = CleanResponse(rawResponse);
        bool taskComplete = toolCalls.Count == 0 && string.IsNullOrEmpty(needMoreContext);

        return new ParsedResponse(
            Content: cleanContent,
            ToolCalls: toolCalls,
            GeneratedFiles: generatedFiles,
            Confidence: confidence,
            TaskComplete: taskComplete,
            NeedMoreContext: needMoreContext);
    }

    private static string ExtractJson(string response)
    {
        int start = response.IndexOf('{');
        int end = response.LastIndexOf('}');

        if (start >= 0 && end > start)
            return response[start..(end + 1)];

        return response;
    }

    private static string CleanResponse(string response)
    {
        string clean = response;

        clean = ExploreAssemblyRegex().Replace(clean, "");
        clean = ExploreFileRegex().Replace(clean, "");
        clean = ExploreFolderRegex().Replace(clean, "");
        clean = ExploreFilesRegex().Replace(clean, "");
        clean = GenerateFileRegex().Replace(clean, "");
        clean = AppendFileRegex().Replace(clean, "");
        clean = OverwriteFileRegex().Replace(clean, "");
        clean = ModifyProjectFileRegex().Replace(clean, "");
        clean = ConfidenceRegex().Replace(clean, "");
        clean = NeedMoreContextRegex().Replace(clean, "");

        while (clean.Contains("\n\n\n"))
            clean = clean.Replace("\n\n\n", "\n\n");

        return clean.Trim();
    }

    [GeneratedRegex(@"\[EXPLORE_ASSEMBLY:\s*name=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreAssemblyRegex();

    [GeneratedRegex(@"\[EXPLORE_FILE:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreFileRegex();

    [GeneratedRegex(@"\[EXPLORE_FOLDER:\s*path=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreFolderRegex();

    [GeneratedRegex(@"\[EXPLORE_FILES:\s*paths=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex ExploreFilesRegex();

    [GeneratedRegex(@"\[GENERATE_FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*([\s\S]*?)\[/GENERATE_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex GenerateFileRegex();

    [GeneratedRegex(@"\[APPEND_FILE:\s*name=""([^""]+)""\]\s*([\s\S]*?)\[/APPEND_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex AppendFileRegex();

    [GeneratedRegex(@"\[OVERWRITE_FILE:\s*name=""([^""]+)""\s+language=""([^""]+)""\]\s*([\s\S]*?)\[/OVERWRITE_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex OverwriteFileRegex();

    [GeneratedRegex(@"\[MODIFY_PROJECT_FILE:\s*path=""([^""]+)""\]\s*([\s\S]*?)\[/MODIFY_PROJECT_FILE\]", RegexOptions.IgnoreCase)]
    private static partial Regex ModifyProjectFileRegex();

    [GeneratedRegex(@"\[CONFIDENCE:\s*([\d.]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex ConfidenceRegex();

    [GeneratedRegex(@"\[NEED_MORE_CONTEXT:\s*reason=""([^""]+)""\]", RegexOptions.IgnoreCase)]
    private static partial Regex NeedMoreContextRegex();
}