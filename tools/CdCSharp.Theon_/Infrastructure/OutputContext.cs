// Infrastructure/OutputContext.cs
namespace CdCSharp.Theon.Infrastructure;

public interface IOutputContext
{
    string CurrentResponseFolder { get; }
    void SetResponseFolder(string folder);
    string? GetGeneratedFileContent(string fileName);
    void UpdateGeneratedFile(string fileName, string content);
    IReadOnlyDictionary<string, string> GetAllGeneratedFiles();
    void Clear();
}

public sealed class OutputContext : IOutputContext
{
    private string _currentFolder = string.Empty;
    private readonly Dictionary<string, string> _generatedFiles = [];

    public string CurrentResponseFolder => _currentFolder;
    public void SetResponseFolder(string folder)
    {
        _currentFolder = folder;
        _generatedFiles.Clear();
    }

    public string? GetGeneratedFileContent(string fileName) =>
        _generatedFiles.TryGetValue(fileName, out string? content) ? content : null;

    public void UpdateGeneratedFile(string fileName, string content) =>
        _generatedFiles[fileName] = content;

    public IReadOnlyDictionary<string, string> GetAllGeneratedFiles() =>
        _generatedFiles;

    public void Clear()
    {
        _currentFolder = string.Empty;
        _generatedFiles.Clear();
    }
}