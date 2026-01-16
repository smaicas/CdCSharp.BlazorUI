using CdCSharp.Theon.Infrastructure;

namespace CdCSharp.Theon.Tools;

public class FileAccessTool
{
    private readonly string _rootPath;
    private readonly IgnoreFilter _ignoreFilter;
    private readonly TheonLogger _logger;

    public FileAccessTool(string rootPath, IgnoreFilter ignoreFilter, TheonLogger logger)
    {
        _rootPath = rootPath;
        _ignoreFilter = ignoreFilter;
        _logger = logger;
    }

    public async Task<string?> GetFileContentAsync(string relativePath)
    {
        string fullPath = Path.Combine(_rootPath, relativePath);

        if (!File.Exists(fullPath))
        {
            _logger.Warning($"File not found: {relativePath}");
            return null;
        }

        if (_ignoreFilter.IsIgnored(fullPath))
        {
            _logger.Warning($"File is ignored: {relativePath}");
            return null;
        }

        try
        {
            string content = await File.ReadAllTextAsync(fullPath);
            _logger.Debug($"Read file: {relativePath} ({content.Length} chars)");
            return content;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to read {relativePath}", ex);
            return null;
        }
    }

    public async Task<Dictionary<string, string>> GetFilesContentAsync(List<string> relativePaths)
    {
        Dictionary<string, string> results = [];

        foreach (string path in relativePaths)
        {
            string? content = await GetFileContentAsync(path);
            if (content != null)
                results[path] = content;
        }

        return results;
    }

    public List<string> ListFiles(string? pattern = null, string? folder = null)
    {
        string searchPath = folder != null ? Path.Combine(_rootPath, folder) : _rootPath;

        if (!Directory.Exists(searchPath))
            return [];

        string searchPattern = pattern ?? "*.*";

        return Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories)
            .Where(f => !_ignoreFilter.IsIgnored(f))
            .Select(f => Path.GetRelativePath(_rootPath, f))
            .ToList();
    }

    public List<string> ListFilesByExtension(string extension)
    {
        string pattern = extension.StartsWith('.') ? $"*{extension}" : $"*.{extension}";
        return ListFiles(pattern);
    }

    public List<string> ListFilesInFolder(string folder)
    {
        return ListFiles(folder: folder);
    }

    public long EstimateTokens(string content)
    {
        return content.Length / 4;
    }

    public async Task<long> EstimateFileTokensAsync(string relativePath)
    {
        string? content = await GetFileContentAsync(relativePath);
        return content != null ? EstimateTokens(content) : 0;
    }
}