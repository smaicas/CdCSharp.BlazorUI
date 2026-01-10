namespace CdCSharp.BlazorUI.Sites.Core.Templates;

public interface ISiteTemplateInstantiator
{
    SiteDefinition CreateSite(
        SiteTemplateDefinition template,
        string siteId,
        string siteName);
}

public sealed class SiteTemplateInstantiator
    : ISiteTemplateInstantiator
{
    public SiteDefinition CreateSite(
        SiteTemplateDefinition template,
        string siteId,
        string siteName)
    {
        return new SiteDefinition
        {
            Id = siteId,
            Name = siteName,
            Pages = template.Pages.Select(p => new PageDefinition
            {
                Id = p.Id,
                Route = p.Route,
                Root = CloneNode(p.Root)
            }).ToList()
        };
    }

    private static UiNode CloneNode(UiNode node)
        => node switch
        {
            LayoutNode layout => layout with
            {
                Id = Guid.NewGuid().ToString("N"),
                Children = layout.Children.Select(CloneNode).ToList()
            },
            ComponentNode component => component with
            {
                Id = Guid.NewGuid().ToString("N")
            },
            _ => throw new NotSupportedException()
        };
}
