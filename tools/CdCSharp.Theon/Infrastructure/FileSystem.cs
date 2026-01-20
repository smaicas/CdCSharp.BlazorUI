namespace CdCSharp.Theon.Infrastructure;

public interface IFileSystem
{
    Task<string?> ReadFileAsync(string relativePath, CancellationToken ct = default);
    Task WriteOutputFileAsync(string responseFolder, string fileName, string content, CancellationToken ct = default);
    Task<bool> WriteProjectFileAsync(string relativePath, string content, CancellationToken ct = default);
    IEnumerable<string> EnumerateFiles(string? relativePath = null, string pattern = "*.*");
    void RegisterObserver(IFileSystemObserver observer);
    void UnregisterObserver(IFileSystemObserver observer);
}

public interface IFileSystemObserver
{
    void OnFileChanged(string relativePath, FileChangeType changeType);
}

public enum FileChangeType
{
    Created,
    Modified,
    Deleted
}

public sealed class FileSystem : IFileSystem
{
    private readonly TheonOptions _options;
    private readonly ITheonLogger _logger;
    private readonly HashSet<string> _ignoredPatterns;
    private readonly List<IFileSystemObserver> _observers = [];
    private readonly object _observersLock = new();

    // Debouncing mechanism for file changes
    private readonly Dictionary<string, (FileChangeType Type, Timer Timer)> _pendingChanges = [];
    private readonly object _changesLock = new();
    private const int DebounceDelayMs = 500;

    public FileSystem(TheonOptions options, ITheonLogger logger)
    {
        _options = options;
        _logger = logger;
        _ignoredPatterns = LoadIgnorePatterns();
    }

    public void RegisterObserver(IFileSystemObserver observer)
    {
        lock (_observersLock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                _logger.Debug($"Observer registered: {observer.GetType().Name}");
            }
        }
    }

    public void UnregisterObserver(IFileSystemObserver observer)
    {
        lock (_observersLock)
        {
            _observers.Remove(observer);
            _logger.Debug($"Observer unregistered: {observer.GetType().Name}");
        }
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
            : GetFullPath(_options.ResponsesPath);

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

        if (_options.Modification.CreateBackup && exists)
        {
            await CreateBackupAsync(relativePath, ct);
        }

        string? dir = Path.GetDirectoryName(fullPath);
        if (dir != null) Directory.CreateDirectory(dir);

        await File.WriteAllTextAsync(fullPath, content, ct);

        _logger.Info($"Project file {(exists ? "modified" : "created")}: {relativePath}");

        FileChangeType changeType = exists ? FileChangeType.Modified : FileChangeType.Created;
        NotifyObserversDebounced(relativePath, changeType);

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

    private string GetFullPath(string relativePath) => Path.Combine(_options.ProjectPath, relativePath);

    private async Task CreateBackupAsync(string relativePath, CancellationToken ct)
    {
        string backupsPath = Path.IsPathRooted(_options.BackupsPath)
            ? _options.BackupsPath
            : GetFullPath(_options.BackupsPath);

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
            "bin/", "obj/", ".vs/", ".vscode/", ".idea/", ".git/",
            "node_modules/", "packages/", "TestResults/", ".theon/"
        ];

        foreach (string ignoreFilePath in _options.IgnoreFiles)
        {
            string currentPath = GetFullPath(ignoreFilePath);
            if (!File.Exists(currentPath)) continue;

            _logger.Info($"Found and applying ignore file {Path.GetFileName(currentPath)}");

            foreach (string line in File.ReadLines(currentPath))
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith('#'))
                {
                    patterns.Add(trimmed);
                }
            }
        }

        return patterns;
    }

    private bool IsIgnored(string path) => _ignoredPatterns.Any(path.Contains);

    /// <summary>
    /// Debounced notification to prevent rapid-fire events.
    /// Multiple changes to the same file within the debounce window are collapsed.
    /// </summary>
    private void NotifyObserversDebounced(string relativePath, FileChangeType changeType)
    {
        lock (_changesLock)
        {
            // Cancel existing timer if present
            if (_pendingChanges.TryGetValue(relativePath, out (FileChangeType Type, Timer Timer) existing))
            {
                existing.Timer.Dispose();
            }

            // Create new timer
            Timer timer = new(
                _ => ProcessFileChange(relativePath, changeType),
                null,
                DebounceDelayMs,
                Timeout.Infinite);

            _pendingChanges[relativePath] = (changeType, timer);
        }
    }

    /// <summary>
    /// Process file change after debounce delay.
    /// Thread-safe: locks to get observers, then notifies outside lock to avoid deadlocks.
    /// </summary>
    private void ProcessFileChange(string relativePath, FileChangeType changeType)
    {
        lock (_changesLock)
        {
            if (_pendingChanges.TryGetValue(relativePath, out (FileChangeType Type, Timer Timer) pending))
            {
                pending.Timer.Dispose();
                _pendingChanges.Remove(relativePath);
            }
        }

        List<IFileSystemObserver> observersCopy;
        lock (_observersLock)
        {
            observersCopy = [.. _observers];
        }

        if (observersCopy.Count == 0)
            return;

        _logger.Debug($"Notifying {observersCopy.Count} observers of {changeType}: {relativePath}");

        // Notify outside lock to prevent deadlocks
        foreach (IFileSystemObserver observer in observersCopy)
        {
            try
            {
                observer.OnFileChanged(relativePath, changeType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Observer notification failed for {observer.GetType().Name}", ex);
            }
        }
    }
}