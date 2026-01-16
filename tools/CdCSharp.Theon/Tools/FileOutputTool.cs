using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Text;

namespace CdCSharp.Theon.Tools;

public class FileOutputTool
{
    private readonly string _outputPath;
    private readonly TheonLogger _logger;
    private int _responseCounter;

    public FileOutputTool(string outputPath, TheonLogger logger)
    {
        _outputPath = Path.Combine(outputPath, "responses");
        _logger = logger;
        Directory.CreateDirectory(_outputPath);

        _responseCounter = GetLastResponseNumber();
    }

    public async Task<ResponseOutput> SaveResponseAsync(
        string query,
        string responseContent,
        List<GeneratedFile> files,
        ResponseMetadata metadata)
    {
        int number = Interlocked.Increment(ref _responseCounter);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        string slug = CreateSlug(query);
        string folderName = $"{number:D3}_{timestamp}_{slug}";
        string folderPath = Path.Combine(_outputPath, folderName);

        Directory.CreateDirectory(folderPath);

        StringBuilder markdown = new();
        markdown.AppendLine($"# Response #{number}");
        markdown.AppendLine();
        markdown.AppendLine($"**Query:** {query}");
        markdown.AppendLine();
        markdown.AppendLine($"**Timestamp:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        markdown.AppendLine();
        markdown.AppendLine($"**Agents:** {string.Join(", ", metadata.AgentsInvolved)}");
        markdown.AppendLine();
        markdown.AppendLine($"**Confidence:** {metadata.FinalConfidence:P0}");
        markdown.AppendLine();
        markdown.AppendLine($"**Processing Time:** {metadata.ProcessingTime.TotalSeconds:F1}s");
        markdown.AppendLine();
        markdown.AppendLine("---");
        markdown.AppendLine();
        markdown.AppendLine(responseContent);

        if (files.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("---");
            markdown.AppendLine();
            markdown.AppendLine("## Generated Files");
            markdown.AppendLine();

            foreach (GeneratedFile file in files)
            {
                markdown.AppendLine($"- `{file.FileName}`");
            }
        }

        string responsePath = Path.Combine(folderPath, "response.md");
        await File.WriteAllTextAsync(responsePath, markdown.ToString());
        _logger.Info($"Saved response to: {folderPath}");

        foreach (GeneratedFile file in files)
        {
            string filePath = Path.Combine(folderPath, file.FileName);
            string? dir = Path.GetDirectoryName(filePath);
            if (dir != null) Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(filePath, file.Content);
            _logger.Debug($"  Generated: {file.FileName}");
        }

        return new ResponseOutput
        {
            FolderPath = folderPath,
            Query = query,
            ResponseMarkdown = markdown.ToString(),
            Files = files,
            Metadata = metadata
        };
    }

    private int GetLastResponseNumber()
    {
        if (!Directory.Exists(_outputPath))
            return 0;

        string[] dirs = Directory.GetDirectories(_outputPath);
        int max = 0;

        foreach (string dir in dirs)
        {
            string name = Path.GetFileName(dir);
            int underscoreIndex = name.IndexOf('_');
            if (underscoreIndex > 0 && int.TryParse(name[..underscoreIndex], out int num))
            {
                if (num > max) max = num;
            }
        }

        return max;
    }

    private static string CreateSlug(string text)
    {
        string slug = text.ToLowerInvariant();

        slug = string.Join("", slug.Where(c => char.IsLetterOrDigit(c) || c == ' '));

        slug = slug.Replace(' ', '-');

        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");

        slug = slug.Trim('-');

        if (slug.Length > 30)
            slug = slug[..30].TrimEnd('-');

        return string.IsNullOrEmpty(slug) ? "response" : slug;
    }
}