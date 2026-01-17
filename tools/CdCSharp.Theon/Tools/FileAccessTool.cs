using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;

namespace CdCSharp.Theon.Tools;

public class FileAccessTool
{
    private readonly string _rootPath;
    private readonly IgnoreFilter _ignoreFilter;
    private readonly TheonLogger _logger;
    private PreAnalysisResult? _preAnalysis;

    public FileAccessTool(string rootPath, IgnoreFilter ignoreFilter, TheonLogger logger)
    {
        _rootPath = rootPath;
        _ignoreFilter = ignoreFilter;
        _logger = logger;
    }

    /// <summary>
    /// Debe ser llamado después del pre-análisis para tener acceso a la estructura
    /// </summary>
    public void SetPreAnalysis(PreAnalysisResult preAnalysis)
    {
        _preAnalysis = preAnalysis;
        _logger.Debug($"FileAccessTool: Pre-analysis structure loaded with {preAnalysis.Structure.Assemblies.Count} assemblies");
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

    /// <summary>
    /// Lista archivos de un ensamblado específico usando la estructura del pre-análisis
    /// </summary>
    public List<string> ListFilesByAssembly(string assemblyName)
    {
        if (_preAnalysis == null)
        {
            _logger.Warning("Pre-analysis not loaded, cannot list files by assembly");
            return [];
        }

        // Normalizar nombre: quitar espacios, case-insensitive
        string normalizedSearch = assemblyName.Trim().ToLowerInvariant();

        // Buscar ensamblado por nombre exacto o parcial
        AssemblyStructure? assembly = _preAnalysis.Structure.Assemblies
            .FirstOrDefault(a => a.Name.Equals(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        if (assembly == null)
        {
            // Búsqueda parcial si no hay match exacto
            assembly = _preAnalysis.Structure.Assemblies
                .FirstOrDefault(a => a.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        if (assembly == null)
        {
            _logger.Warning($"Assembly not found: {assemblyName}");
            _logger.Debug($"Available assemblies: {string.Join(", ", _preAnalysis.Structure.Assemblies.Select(a => a.Name))}");
            return [];
        }

        // Retornar todos los archivos registrados
        List<string> files = [];
        files.AddRange(assembly.Files.CSharp);
        files.AddRange(assembly.Files.Razor);
        files.AddRange(assembly.Files.TypeScript);
        files.AddRange(assembly.Files.Css);

        _logger.Debug($"Found {files.Count} files in assembly '{assembly.Name}'");
        return files;
    }

    /// <summary>
    /// Lista todos los archivos del proyecto (todos los ensamblados no-test)
    /// </summary>
    public List<string> ListAllProjectFiles()
    {
        if (_preAnalysis == null)
        {
            _logger.Warning("Pre-analysis not loaded, falling back to filesystem scan");
            return ListFiles();
        }

        List<string> allFiles = [];

        foreach (AssemblyStructure assembly in _preAnalysis.Structure.Assemblies.Where(a => !a.IsTestProject))
        {
            allFiles.AddRange(assembly.Files.CSharp);
            allFiles.AddRange(assembly.Files.Razor);
            allFiles.AddRange(assembly.Files.TypeScript);
            allFiles.AddRange(assembly.Files.Css);
        }

        _logger.Debug($"Listed {allFiles.Count} files from {_preAnalysis.Structure.Assemblies.Count(a => !a.IsTestProject)} non-test assemblies");
        return allFiles.Distinct().ToList();
    }

    /// <summary>
    /// Fallback: lista archivos escaneando el filesystem (legacy)
    /// </summary>
    public List<string> ListFiles(string? pattern = null, string? folder = null)
    {
        string searchPath = folder != null ? Path.Combine(_rootPath, folder) : _rootPath;

        if (!Directory.Exists(searchPath))
        {
            _logger.Warning($"Directory does not exist: {searchPath}");
            return [];
        }

        string searchPattern = pattern ?? "*.*";

        return Directory.GetFiles(searchPath, searchPattern, SearchOption.AllDirectories)
            .Where(f => !_ignoreFilter.IsIgnored(f))
            .Select(f => Path.GetRelativePath(_rootPath, f))
            .ToList();
    }

    public List<string> ListFilesByExtension(string extension)
    {
        if (_preAnalysis == null)
        {
            string pattern = extension.StartsWith('.') ? $"*{extension}" : $"*.{extension}";
            return ListFiles(pattern);
        }

        // Usar estructura del pre-análisis
        string ext = extension.TrimStart('.').ToLowerInvariant();

        List<string> files = [];
        foreach (AssemblyStructure assembly in _preAnalysis.Structure.Assemblies.Where(a => !a.IsTestProject))
        {
            switch (ext)
            {
                case "cs":
                    files.AddRange(assembly.Files.CSharp);
                    break;
                case "razor":
                    files.AddRange(assembly.Files.Razor);
                    break;
                case "ts":
                case "tsx":
                    files.AddRange(assembly.Files.TypeScript);
                    break;
                case "css":
                case "scss":
                    files.AddRange(assembly.Files.Css);
                    break;
            }
        }

        return files.Distinct().ToList();
    }

    /// <summary>
    /// DEPRECATED: Use ListFilesByAssembly instead
    /// </summary>
    [Obsolete("Use ListFilesByAssembly for assembly-based queries")]
    public List<string> ListFilesInFolder(string folder)
    {
        _logger.Warning($"ListFilesInFolder is deprecated. Folder: {folder}");
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