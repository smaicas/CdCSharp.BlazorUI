using System.Text.RegularExpressions;

namespace CdCSharp.Theon.Core;

public interface IToolParser
{
    ParseResult Parse(string response);
}

public sealed record ParseResult(
    string CleanContent,
    IReadOnlyList<ToolInvocation> Tools,
    float Confidence,
    bool NeedsMoreContext,
    string? MoreContextReason);

public sealed partial class ToolParser : IToolParser
{
    public ParseResult Parse(string response)
    {
        List<ToolInvocation> tools = [];
        float confidence = 0f;
        bool needsMore = false;
        string? moreReason = null;

        foreach (Match m in ExploreAssemblyRegex().Matches(response))
            tools.Add(new ExploreAssemblyTool(m.Groups[1].Value));

        foreach (Match m in ExploreFileRegex().Matches(response))
            tools.Add(new ExploreFileTool(m.Groups[1].Value));

        foreach (Match m in ExploreFolderRegex().Matches(response))
            tools.Add(new ExploreFolderTool(m.Groups[1].Value));

        foreach (Match m in ExploreFilesRegex().Matches(response))
        {
            List<string> paths = m.Groups[1].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            tools.Add(new ExploreFilesTool(paths));
        }

        foreach (Match m in GenerateFileRegex().Matches(response))
            tools.Add(new GenerateFileTool(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value.Trim()));

        foreach (Match m in AppendFileRegex().Matches(response))
            tools.Add(new AppendFileTool(m.Groups[1].Value, m.Groups[2].Value.Trim()));

        foreach (Match m in OverwriteFileRegex().Matches(response))
            tools.Add(new OverwriteFileTool(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value.Trim()));

        foreach (Match m in ModifyProjectFileRegex().Matches(response))
            tools.Add(new ModifyProjectFileTool(m.Groups[1].Value, m.Groups[2].Value.Trim()));

        Match confMatch = ConfidenceRegex().Match(response);
        if (confMatch.Success && float.TryParse(confMatch.Groups[1].Value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float conf))
        {
            confidence = Math.Clamp(conf, 0f, 1f);
        }

        Match needMoreMatch = NeedMoreContextRegex().Match(response);
        if (needMoreMatch.Success)
        {
            needsMore = true;
            moreReason = needMoreMatch.Groups[1].Value;
        }

        string clean = CleanResponse(response);

        return new ParseResult(clean, tools, confidence, needsMore, moreReason);
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