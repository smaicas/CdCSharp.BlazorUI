namespace CdCSharp.BlazorUI.Sites.Core;

public sealed record SiteDefinition
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public IReadOnlyList<PageDefinition> Pages { get; init; } = [];
}

public sealed record PageDefinition
{
    public string Id { get; init; } = default!;
    public string Route { get; init; } = "/";
    public UiNode Root { get; init; } = default!;
}

public abstract record UiNode
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public Dictionary<string, NodeProp> Props { get; init; }
        = [];
}

public sealed record LayoutNode : UiNode
{
    public IReadOnlyList<UiNode> Children { get; init; }
        = [];
}

public sealed record ComponentNode : UiNode
{
    public string ComponentKey { get; init; } = default!;
}

public sealed record NodeProp
{
    public string Name { get; init; } = default!;
    public NodePropType Type { get; init; }
    public object? Value { get; init; }
}

public enum NodePropType
{
    String,
    Number,
    Boolean,
    Enum,
    Color,
    Json
}

public sealed record ComponentMetadata
{
    public string Key { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Category { get; init; } = default!;
    public IReadOnlyList<ComponentPropMetadata> Props { get; init; }
        = [];
}

public sealed record ComponentPropMetadata
{
    public string Name { get; init; } = default!;
    public NodePropType Type { get; init; }
    public object? DefaultValue { get; init; }
    public bool Required { get; init; }
}

public interface IComponentRegistry
{
    /// <summary>
    /// Resuelve un componente registrado por su clave única.
    /// </summary>
    ComponentRegistration Resolve(string key);

    /// <summary>
    /// Devuelve todos los componentes disponibles (para el builder).
    /// </summary>
    IReadOnlyCollection<ComponentRegistration> GetAll();
}

public sealed record ComponentRegistration
{
    /// <summary>
    /// Clave única y estable del componente (ej: "input.text").
    /// </summary>
    public string Key { get; init; } = default!;

    /// <summary>
    /// Metadata usada por el builder y validación.
    /// </summary>
    public ComponentMetadata Metadata { get; init; } = default!;
}