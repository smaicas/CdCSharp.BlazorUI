using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using System.Text;

namespace CdCSharp.Theon.Context;

public interface IContextScope
{
    string Name { get; }
    ContextType Type { get; }
    int EstimatedTokens { get; }
    string BuildContext();
}

public sealed class ProjectScope : IContextScope
{
    private readonly ProjectInfo _project;

    public string Name => _project.Name;
    public ContextType Type => ContextType.Project;
    public int EstimatedTokens { get; }

    private readonly string _context;

    public ProjectScope(ProjectInfo project)
    {
        _project = project;
        _context = BuildProjectContext();
        EstimatedTokens = _context.Length / 4;
    }

    public string BuildContext() => _context;

    private string BuildProjectContext()
    {
        StringBuilder sb = new();

        foreach (AssemblyInfo assembly in _project.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"ASSEMBLY: {assembly.Name}");
            sb.AppendLine($"  Path: {assembly.RelativePath}");
            sb.AppendLine($"  Files ({assembly.Files.Count}):");
            foreach (string file in assembly.Files.Take(50))
                sb.AppendLine($"    - {file}");
            if (assembly.Files.Count > 50)
                sb.AppendLine($"    ... and {assembly.Files.Count - 50} more");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public sealed class AssemblyScope : IContextScope
{
    public string Name { get; }
    public ContextType Type => ContextType.Assembly;
    public int EstimatedTokens { get; }

    private readonly string _context;

    public AssemblyScope(AssemblyInfo assembly)
    {
        Name = assembly.Name;
        _context = BuildAssemblyContext(assembly);
        EstimatedTokens = _context.Length / 4;
    }

    public string BuildContext() => _context;

    private static string BuildAssemblyContext(AssemblyInfo assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"ASSEMBLY: {assembly.Name}");
        sb.AppendLine($"Path: {assembly.RelativePath}");
        sb.AppendLine($"References: {string.Join(", ", assembly.References)}");
        sb.AppendLine();

        sb.AppendLine("FILES:");
        foreach (string file in assembly.Files)
            sb.AppendLine($"  - {file}");
        sb.AppendLine();

        sb.AppendLine("TYPES:");
        IEnumerable<IGrouping<string, TypeSummary>> byNamespace = assembly.Types.GroupBy(t => t.Namespace);
        foreach (IGrouping<string, TypeSummary> ns in byNamespace.OrderBy(g => g.Key))
        {
            sb.AppendLine($"  namespace {ns.Key}:");
            foreach (TypeSummary type in ns.OrderBy(t => t.Name))
                sb.AppendLine($"    {type.Kind}: {type.Name} ({type.FilePath})");
        }

        return sb.ToString();
    }
}

public sealed class FileScope : IContextScope
{
    public string Name { get; }
    public ContextType Type => ContextType.File;
    public int EstimatedTokens { get; }

    private readonly string _context;

    public FileScope(string relativePath, string content)
    {
        Name = relativePath;
        _context = $"FILE: {relativePath}\n\n{content}";
        EstimatedTokens = _context.Length / 4;
    }

    public string BuildContext() => _context;
}

public sealed class FolderScope : IContextScope
{
    public string Name { get; }
    public ContextType Type => ContextType.Folder;
    public int EstimatedTokens { get; }

    private readonly string _context;

    public FolderScope(string folderPath, IReadOnlyDictionary<string, string> fileContents, int maxTokens)
    {
        Name = folderPath;
        _context = BuildFolderContext(folderPath, fileContents, maxTokens);
        EstimatedTokens = _context.Length / 4;
    }

    public string BuildContext() => _context;

    private static string BuildFolderContext(string folderPath, IReadOnlyDictionary<string, string> fileContents, int maxTokens)
    {
        StringBuilder sb = new();
        sb.AppendLine($"FOLDER: {folderPath}");
        sb.AppendLine($"Files: {fileContents.Count}");
        sb.AppendLine();

        int currentTokens = sb.Length / 4;

        foreach ((string path, string content) in fileContents.OrderBy(kvp => kvp.Key))
        {
            int fileTokens = content.Length / 4;
            if (currentTokens + fileTokens > maxTokens)
            {
                sb.AppendLine($"FILE: {path} (truncated - {content.Length} chars)");
                sb.AppendLine();
                continue;
            }

            sb.AppendLine($"FILE: {path}");
            sb.AppendLine(content);
            sb.AppendLine();
            currentTokens += fileTokens;
        }

        return sb.ToString();
    }
}

public sealed class MultiFileScope : IContextScope
{
    public string Name { get; }
    public ContextType Type => ContextType.MultiFile;
    public int EstimatedTokens { get; }

    private readonly string _context;

    public MultiFileScope(IReadOnlyList<string> paths, IReadOnlyDictionary<string, string> fileContents)
    {
        Name = $"{paths.Count} files";
        _context = BuildMultiFileContext(paths, fileContents);
        EstimatedTokens = _context.Length / 4;
    }

    public string BuildContext() => _context;

    private static string BuildMultiFileContext(IReadOnlyList<string> paths, IReadOnlyDictionary<string, string> fileContents)
    {
        StringBuilder sb = new();
        sb.AppendLine($"FILES: {string.Join(", ", paths)}");
        sb.AppendLine();

        foreach (string path in paths)
        {
            if (fileContents.TryGetValue(path, out string? content))
            {
                sb.AppendLine($"FILE: {path}");
                sb.AppendLine(content);
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"FILE: {path} (not found)");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}