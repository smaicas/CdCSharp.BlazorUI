namespace CdCSharp.DocGen.Core.Models;

public record ProjectInfo
{
    public required string Name { get; init; }
    public required string RootPath { get; init; }
    public ProjectType Type { get; init; }
    public List<FileInfo> Files { get; init; } = [];
    public List<TypeInfo> PublicTypes { get; init; } = [];
    public List<PatternInfo> Patterns { get; init; } = [];
}

public enum ProjectType
{
    ClassLibrary,
    BlazorComponent,
    WebApi,
    Console,
    Unknown
}

public record FileInfo
{
    public required string RelativePath { get; init; }
    public required FileType Type { get; init; }
    public int LineCount { get; init; }
    public int TokenEstimate { get; init; }
    public List<string> PublicSymbols { get; init; } = [];
    public ImportanceLevel Importance { get; init; } = ImportanceLevel.Normal;
}

public enum FileType
{
    CSharp,
    Razor,
    TypeScript,
    JavaScript,
    Css,
    Scss,
    Config,
    Markdown,
    Unknown
}

public enum ImportanceLevel
{
    Critical,
    High,
    Normal,
    Low
}

public record TypeInfo
{
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string FilePath { get; init; }
    public TypeKind Kind { get; init; }
    public List<MemberInfo> PublicMembers { get; init; } = [];
    public List<string> BaseTypes { get; init; } = [];
    public List<AttributeInfo> Attributes { get; init; } = [];
    public ImportanceLevel Importance { get; init; } = ImportanceLevel.Normal;
    public string? AiSummary { get; init; }
}

public enum TypeKind
{
    Class,
    Interface,
    Struct,
    Record,
    Enum,
    Delegate
}

public static class TypeKindExtensions
{
    public static string ToPlural(this TypeKind kind)
    {
        return kind switch
        {
            TypeKind.Class => "Classes",
            TypeKind.Interface => "Interfaces",
            TypeKind.Struct => "Structs",
            TypeKind.Record => "Records",
            TypeKind.Enum => "Enums",
            TypeKind.Delegate => "Delegates",
            _ => kind.ToString() + "s" // Fallback por si añades más en el futuro
        };
    }
}

public record MemberInfo
{
    public required string Name { get; init; }
    public required string Signature { get; init; }
    public MemberKind Kind { get; init; }
}

public enum MemberKind
{
    Method,
    Property,
    Field,
    Event,
    Constructor
}

public record AttributeInfo
{
    public required string Name { get; init; }
    public List<string> Arguments { get; init; } = [];
}

public record ComponentInfo
{
    public required string Name { get; init; }
    public required string FilePath { get; init; }
    public List<ParameterInfo> Parameters { get; init; } = [];
    public bool HasCodeBlock { get; init; }
}

public record ParameterInfo
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public bool IsRequired { get; init; }
}

public record PatternInfo
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public List<string> AffectedFiles { get; init; } = [];
    public PatternType Type { get; init; }
}

public enum PatternType
{
    SourceGenerator,
    DependencyInjection,
    Repository,
    Factory,
    Blazor,
    Other
}