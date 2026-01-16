namespace CdCSharp.Theon.Models;

public record ProjectStructure
{
    public string Solution { get; init; } = string.Empty;
    public string RootPath { get; init; } = string.Empty;
    public List<AssemblyStructure> Assemblies { get; init; } = [];
    public ProjectSummary Summary { get; init; } = new();
}

public record AssemblyStructure
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public bool IsTestProject { get; init; }
    public List<string> References { get; init; } = [];
    public List<NamespaceInfo> Namespaces { get; init; } = [];
    public FileCollection Files { get; init; } = new();
}

public record NamespaceInfo
{
    public string Name { get; init; } = string.Empty;
    public List<TypeInfo> Types { get; init; } = [];
}

public record TypeInfo
{
    public string Name { get; init; } = string.Empty;
    public TypeKind Kind { get; init; }
    public string File { get; init; } = string.Empty;
    public List<string> Modifiers { get; init; } = [];
    public List<string> BaseTypes { get; init; } = [];
    public List<MemberInfo> Members { get; init; } = [];
}

public record MemberInfo
{
    public string Name { get; init; } = string.Empty;
    public MemberKind Kind { get; init; }
    public string Signature { get; init; } = string.Empty;
    public List<string> Modifiers { get; init; } = [];
}

public enum TypeKind { Class, Interface, Record, Struct, Enum, Delegate }
public enum MemberKind { Constructor, Method, Property, Field, Event }

public record FileCollection
{
    public List<string> CSharp { get; init; } = [];
    public List<string> Razor { get; init; } = [];
    public List<string> TypeScript { get; init; } = [];
    public List<string> Css { get; init; } = [];
    public List<string> Other { get; init; } = [];
}

public record ProjectSummary
{
    public int TotalAssemblies { get; init; }
    public int TotalTypes { get; init; }
    public int TotalFiles { get; init; }
    public List<string> DetectedPatterns { get; init; } = [];
    public string ProjectType { get; init; } = "Unknown";
}