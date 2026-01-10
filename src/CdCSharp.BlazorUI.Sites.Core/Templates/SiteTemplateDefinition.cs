namespace CdCSharp.BlazorUI.Sites.Core.Templates;

public sealed record SiteTemplateDefinition
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;

    public IReadOnlyList<PageTemplateDefinition> Pages { get; init; } = [];
}

public sealed record PageTemplateDefinition
{
    public string Id { get; init; } = default!;
    public string Route { get; init; } = "/";
    public UiNode Root { get; init; } = default!;
}
