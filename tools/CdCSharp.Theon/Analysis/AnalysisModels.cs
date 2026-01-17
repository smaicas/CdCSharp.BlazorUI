namespace CdCSharp.Theon.Analysis;

public sealed record ProjectInfo(
    string Name,
    string RootPath,
    IReadOnlyList<AssemblyInfo> Assemblies)
{
    public IEnumerable<string> AllFiles => Assemblies
        .Where(a => !a.IsTestProject)
        .SelectMany(a => a.Files);
}

public sealed record AssemblyInfo(
    string Name,
    string RelativePath,
    bool IsTestProject,
    IReadOnlyList<string> References,
    IReadOnlyList<string> Files,
    IReadOnlyList<TypeSummary> Types);

public sealed record TypeSummary(
    string Namespace,
    string Name,
    TypeKind Kind,
    string FilePath);

public enum TypeKind
{
    Class,
    Interface,
    Record,
    Struct,
    Enum
}
