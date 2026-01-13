namespace CdCSharp.BuildTools;

public class BuildContext
{
    public BuildContext(string projectPath)
    {
        ProjectPath = projectPath;
    }

    public string CssBundlePath => Path.Combine(ProjectPath, "CssBundle");
    public string OutputCssPath => Path.Combine(WwwRootPath, "css");
    public string OutputJsPath => Path.Combine(WwwRootPath, "js");
    public string ProjectPath { get; }
    public string TypesPath => Path.Combine(ProjectPath, "Types");
    public string WwwRootPath => Path.Combine(ProjectPath, "wwwroot");

    public void EnsureDirectoriesFromConfig()
    {
        EnsureDirectory(CssBundlePath);
        EnsureDirectory(WwwRootPath);
        EnsureDirectory(TypesPath);
    }

    public void EnsureDirectory(string relativePath)
    {
        string fullPath = Path.Combine(ProjectPath, relativePath);
        Directory.CreateDirectory(fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(ProjectPath, relativePath);
    }
}