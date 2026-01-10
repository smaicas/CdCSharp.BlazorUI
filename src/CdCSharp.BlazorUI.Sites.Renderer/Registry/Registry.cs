using CdCSharp.BlazorUI.Sites.Core;

namespace CdCSharp.BlazorUI.Sites.Renderer.Registry;

public interface IBlazorComponentRegistry : IComponentRegistry
{
    Type ResolveType(string key);
}

public sealed record BlazorComponentRegistration
{
    public ComponentRegistration Definition { get; init; } = default!;
    public Type ComponentType { get; init; } = default!;
}

public sealed class BlazorComponentRegistry : IBlazorComponentRegistry
{
    private readonly Dictionary<string, BlazorComponentRegistration> _map = [];

    public void Register<TComponent>(
        string key,
        ComponentMetadata metadata)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        _map[key] = new BlazorComponentRegistration
        {
            Definition = new ComponentRegistration
            {
                Key = key,
                Metadata = metadata
            },
            ComponentType = typeof(TComponent)
        };
    }

    public ComponentRegistration Resolve(string key)
        => _map[key].Definition;

    public Type ResolveType(string key)
        => _map[key].ComponentType;

    public IReadOnlyCollection<ComponentRegistration> GetAll()
        => _map.Values.Select(v => v.Definition).ToList();
}