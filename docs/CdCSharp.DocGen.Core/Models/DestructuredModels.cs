using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models;

public record DestructuredAssembly
{
    [JsonPropertyName("assembly")]
    public string Assembly { get; init; } = string.Empty;

    [JsonPropertyName("namespaces")]
    public List<DestructuredNamespace> Namespaces { get; init; } = [];

    [JsonPropertyName("components")]
    public List<DestructuredComponent> Components { get; init; } = [];

    [JsonPropertyName("typescript")]
    public List<DestructuredTypeScript> TypeScript { get; init; } = [];

    [JsonPropertyName("css")]
    public List<DestructuredCss> Css { get; init; } = [];
}

public record DestructuredNamespace
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("types")]
    public List<DestructuredType> Types { get; init; } = [];
}

public record DestructuredType
{
    [JsonPropertyName("kind")]
    public TypeKind Kind { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; init; } = [];

    [JsonPropertyName("base")]
    public List<string> Base { get; init; } = [];

    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; init; } = [];

    [JsonPropertyName("members")]
    public List<DestructuredMember> Members { get; init; } = [];

    [JsonPropertyName("nestedTypes")]
    public List<DestructuredType> NestedTypes { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypeKind
{
    Class,
    Interface,
    Record,
    Struct,
    Enum,
    Delegate
}

public record DestructuredMember
{
    [JsonPropertyName("kind")]
    public MemberKind Kind { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("signature")]
    public string Signature { get; init; } = string.Empty;

    [JsonPropertyName("modifiers")]
    public List<string> Modifiers { get; init; } = [];

    [JsonPropertyName("attributes")]
    public List<string> Attributes { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberKind
{
    Constructor,
    Method,
    Property,
    Field,
    Event,
    Indexer
}

public record DestructuredComponent
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    [JsonPropertyName("hasCodeBehind")]
    public bool HasCodeBehind { get; init; }

    [JsonPropertyName("inherits")]
    public string? Inherits { get; init; }

    [JsonPropertyName("implements")]
    public List<string> Implements { get; init; } = [];

    [JsonPropertyName("parameters")]
    public List<ComponentParameter> Parameters { get; init; } = [];

    [JsonPropertyName("cascadingParameters")]
    public List<ComponentParameter> CascadingParameters { get; init; } = [];

    [JsonPropertyName("injectables")]
    public List<InjectableService> Injectables { get; init; } = [];

    [JsonPropertyName("eventCallbacks")]
    public List<string> EventCallbacks { get; init; } = [];

    [JsonPropertyName("renderFragments")]
    public List<string> RenderFragments { get; init; } = [];
}

public record ComponentParameter
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; init; }

    [JsonPropertyName("editorRequired")]
    public bool EditorRequired { get; init; }

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; init; }
}

public record InjectableService
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

public record DestructuredTypeScript
{
    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    [JsonPropertyName("exports")]
    public List<TsExport> Exports { get; init; } = [];

    [JsonPropertyName("imports")]
    public List<TsImport> Imports { get; init; } = [];
}

public record TsExport
{
    [JsonPropertyName("kind")]
    public TsExportKind Kind { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("signature")]
    public string? Signature { get; init; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TsExportKind
{
    Function,
    Class,
    Interface,
    Type,
    Const,
    Enum
}

public record TsImport
{
    [JsonPropertyName("from")]
    public string From { get; init; } = string.Empty;

    [JsonPropertyName("names")]
    public List<string> Names { get; init; } = [];
}

public record DestructuredCss
{
    [JsonPropertyName("file")]
    public string File { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public CssFileType Type { get; init; }

    [JsonPropertyName("variables")]
    public List<CssVariable> Variables { get; init; } = [];

    [JsonPropertyName("selectors")]
    public List<string> Selectors { get; init; } = [];

    [JsonPropertyName("imports")]
    public List<string> Imports { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CssFileType
{
    Css,
    Scss,
    Less
}

public record CssVariable
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; init; } = ":root";
}