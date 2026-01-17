// Tools/FileOutputTool.cs
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
using System.Text;

namespace CdCSharp.Theon.Tools;

public class FileOutputTool
{
    private readonly string _outputPath;
    private readonly TheonLogger _logger;
    private readonly AgentVisualizer _visualizer;
    private int _responseCounter;

    public FileOutputTool(string outputPath, TheonLogger logger, AgentVisualizer visualizer)
    {
        _outputPath = Path.Combine(outputPath, "responses");
        _logger = logger;
        _visualizer = visualizer;
        Directory.CreateDirectory(_outputPath);

        _responseCounter = GetLastResponseNumber();
    }

    public async Task<ResponseOutput> SaveResponseAsync(
    string query,
    string responseContent,
    List<GeneratedFile> files,
    ResponseMetadata metadata,
    List<AgentInteraction>? interactions = null)
    {
        int number = Interlocked.Increment(ref _responseCounter);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        string slug = CreateSlug(query);
        string folderName = $"{number:D3}_{timestamp}_{slug}";
        string folderPath = Path.Combine(_outputPath, folderName);

        Directory.CreateDirectory(folderPath);

        _logger.Info($"=== Saving Response #{number} ===");
        _logger.Info($"  Output folder: {folderPath}");
        _logger.Info($"  Files to write: {files.Count}");

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

        if (metadata.ValidationRounds > 0)
        {
            markdown.AppendLine($"**Validation Rounds:** {metadata.ValidationRounds}");
            markdown.AppendLine();
        }

        if (metadata.AgentsInvolved.Count > 1)
        {
            markdown.AppendLine("---");
            markdown.AppendLine();
            markdown.AppendLine("## Agent Interaction");
            markdown.AppendLine();

            string mermaidDiagram = _visualizer.GenerateMermaidDiagram(null);
            markdown.AppendLine(mermaidDiagram);
            markdown.AppendLine();

            if (interactions != null && interactions.Count > 0)
            {
                string sequenceDiagram = _visualizer.GenerateInteractionDiagram(interactions);
                if (!string.IsNullOrEmpty(sequenceDiagram))
                {
                    markdown.AppendLine("### Interaction Flow");
                    markdown.AppendLine();
                    markdown.AppendLine(sequenceDiagram);
                    markdown.AppendLine();
                }
            }
        }

        markdown.AppendLine("---");
        markdown.AppendLine();
        markdown.AppendLine("## Response");
        markdown.AppendLine();
        markdown.AppendLine(responseContent);
        markdown.AppendLine();

        if (files.Count > 0)
        {
            markdown.AppendLine();
            markdown.AppendLine("---");
            markdown.AppendLine();
            markdown.AppendLine("## Generated Files");
            markdown.AppendLine();

            foreach (GeneratedFile file in files)
            {
                markdown.AppendLine($"### {file.FileName}");
                markdown.AppendLine();
                markdown.AppendLine($"**Language:** {file.Language}");
                markdown.AppendLine();
                markdown.AppendLine($"**Size:** {file.Content.Length} characters");
                markdown.AppendLine();
                // Opcional: Incluir preview del contenido
                if (file.Content.Length < 500)
                {
                    markdown.AppendLine("**Preview:**");
                    markdown.AppendLine();
                    markdown.AppendLine(file.Content);
                }
                else
                {
                    markdown.AppendLine($"**Preview:** (First 500 characters)");
                    markdown.AppendLine();
                    markdown.AppendLine(file.Content.Substring(0, 500) + "...");
                }
                markdown.AppendLine();
            }
        }

        string responsePath = Path.Combine(folderPath, "response.md");
        await File.WriteAllTextAsync(responsePath, markdown.ToString(), System.Text.Encoding.UTF8);
        _logger.Debug($"Written: response.md to: {folderPath} ({markdown.Length} chars)");

        int writtenFiles = 0;
        int failedFiles = 0;

        foreach (GeneratedFile file in files)
        {
            try
            {
                string filePath = Path.Combine(folderPath, file.FileName);
                string? dir = Path.GetDirectoryName(filePath);
                if (dir != null) Directory.CreateDirectory(dir);

                await File.WriteAllTextAsync(filePath, file.Content);
                writtenFiles++;
                _logger.Debug($"Written: {file.FileName} ({file.Content.Length} chars)");
            }
            catch (Exception ex)
            {
                failedFiles++;
                _logger.Error($"Failed to write: {file.FileName}", ex);
            }
        }

        _logger.Info($"=== Response #{number} Complete ===");
        _logger.Info($"  Written: {writtenFiles} files");
        if (failedFiles > 0)
            _logger.Warning($"  Failed: {failedFiles} files");

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