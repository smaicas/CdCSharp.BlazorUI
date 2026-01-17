namespace CdCSharp.Theon.Infrastructure;

public interface IFileSystem
{
    string ProjectPath { get; }

    Task<string?> ReadFileAsync(string relativePath, CancellationToken ct = default);
    Task WriteOutputFileAsync(string responseFolder, string fileName, string content, CancellationToken ct = default);
    Task<bool> WriteProjectFileAsync(string relativePath, string content, CancellationToken ct = default);

    IEnumerable<string> EnumerateFiles(string? relativePath = null, string pattern = "*.*");
    IEnumerable<string> EnumerateDirectories(string? relativePath = null);
    bool FileExists(string relativePath);
    bool DirectoryExists(string relativePath);

    string GetFullPath(string relativePath);
}

public sealed class FileSystem : IFileSystem
{
    private readonly TheonOptions _options;
    private readonly ITheonLogger _logger;
    private readonly HashSet<string> _ignoredPatterns;

    public string ProjectPath => _options.ProjectPath;

    public FileSystem(TheonOptions options, ITheonLogger logger)
    {
        _options = options;
        _logger = logger;
        _ignoredPatterns = LoadIgnorePatterns();
    }

    public async Task<string?> ReadFileAsync(string relativePath, CancellationToken ct = default)
    {
        string fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            _logger.Warning($"File not found: {relativePath}");
            return null;
        }

        return await File.ReadAllTextAsync(fullPath, ct);
    }

    public async Task WriteOutputFileAsync(string responseFolder, string fileName, string content, CancellationToken ct = default)
    {
        string responsesPath = Path.IsPathRooted(_options.ResponsesPath)
            ? _options.ResponsesPath
            : Path.Combine(_options.ProjectPath, _options.ResponsesPath);

        string folderPath = Path.Combine(responsesPath, responseFolder);
        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, fileName);
        string? dir = Path.GetDirectoryName(filePath);
        if (dir != null) Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(filePath, content, ct);
        _logger.Debug($"Output file written: {fileName}");
    }

    public async Task<bool> WriteProjectFileAsync(string relativePath, string content, CancellationToken ct = default)
    {
        if (!_options.Modification.Enabled)
        {
            _logger.Warning("Project modification is disabled");
            return false;
        }

        string fullPath = GetFullPath(relativePath);
        bool exists = File.Exists(fullPath);

        if (!exists && !_options.Modification.AllowNewFiles)
        {
            _logger.Warning($"Cannot create new project file (disabled): {relativePath}");
            return false;
        }

        if (_options.Modification.CreateBackup && exists)
        {
            await CreateBackupAsync(relativePath, ct);
        }

        string? dir = Path.GetDirectoryName(fullPath);
        if (dir != null) Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(fullPath, content, ct);
        _logger.Info($"Project file {(exists ? "modified" : "created")}: {relativePath}");
        return true;
    }

    public IEnumerable<string> EnumerateFiles(string? relativePath = null, string pattern = "*.*")
    {
        string basePath = relativePath == null
            ? _options.ProjectPath
            : GetFullPath(relativePath);

        if (!Directory.Exists(basePath))
            return [];

        return Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories)
            .Where(f => !IsIgnored(f))
            .Select(f => Path.GetRelativePath(_options.ProjectPath, f));
    }

    public IEnumerable<string> EnumerateDirectories(string? relativePath = null)
    {
        string basePath = relativePath == null
            ? _options.ProjectPath
            : GetFullPath(relativePath);

        if (!Directory.Exists(basePath))
            return [];

        return Directory.EnumerateDirectories(basePath)
            .Where(d => !IsIgnored(d))
            .Select(d => Path.GetRelativePath(_options.ProjectPath, d));
    }

    public bool FileExists(string relativePath) => File.Exists(GetFullPath(relativePath));
    public bool DirectoryExists(string relativePath) => Directory.Exists(GetFullPath(relativePath));
    public string GetFullPath(string relativePath) => Path.Combine(_options.ProjectPath, relativePath);

    private async Task CreateBackupAsync(string relativePath, CancellationToken ct)
    {
        string backupsPath = Path.IsPathRooted(_options.BackupsPath)
            ? _options.BackupsPath
            : Path.Combine(_options.ProjectPath, _options.BackupsPath);

        Directory.CreateDirectory(backupsPath);

        string fileName = Path.GetFileName(relativePath);
        string backupName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}";
        string backupPath = Path.Combine(backupsPath, backupName);

        string content = await File.ReadAllTextAsync(GetFullPath(relativePath), ct);
        await File.WriteAllTextAsync(backupPath, content, ct);
        _logger.Debug($"Backup created: {backupName}");
    }

    private HashSet<string> LoadIgnorePatterns()
    {
        HashSet<string> patterns =
        [
            "bin", "obj", ".vs", ".vscode", ".idea", ".git",
            "node_modules", "packages", "TestResults", ".theon"
        ];

        string gitignorePath = Path.Combine(_options.ProjectPath, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            foreach (string line in File.ReadLines(gitignorePath))
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith('#'))
                {
                    patterns.Add(trimmed.TrimEnd('/'));
                }
            }
        }

        return patterns;
    }

    private bool IsIgnored(string path)
    {
        string relativePath = Path.GetRelativePath(_options.ProjectPath, path);
        string[] parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => _ignoredPatterns.Contains(p));
    }
}