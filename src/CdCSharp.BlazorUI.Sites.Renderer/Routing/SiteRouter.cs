using CdCSharp.BlazorUI.Sites.Core;

namespace CdCSharp.BlazorUI.Sites.Renderer.Routing;

public sealed class SiteRouter
{
    private readonly Dictionary<string, PageDefinition> _routes;

    public SiteRouter(SiteDefinition site)
    {
        _routes = site.Pages.ToDictionary(p => p.Route);
    }

    public PageDefinition? Resolve(string path)
        => _routes.TryGetValue(path, out PageDefinition? page)
            ? page
            : null;
}
