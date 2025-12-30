namespace CdCSharp.BlazorUI.BuildTools.Pipeline;

public class BuildContext
{
    public string ProjectPath { get; }
    public string CssBundlePath => Path.Combine(ProjectPath, "CssBundle");
    public string WwwRootPath => Path.Combine(ProjectPath, "wwwroot");
    public string TypesPath => Path.Combine(ProjectPath, "Types");

    public BuildContext(string projectPath)
    {
        ProjectPath = projectPath;
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
