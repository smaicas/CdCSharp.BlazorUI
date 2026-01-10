using CdCSharp.BlazorUI.Sites.Core;
using CdCSharp.BlazorUI.Sites.Core.Templates;

namespace CdCSharp.BlazorUI.Sites.AppTest.Wasm.Templates;

public static class DocsTemplate
{
    public static SiteTemplateDefinition Create()
        => new()
        {
            Id = "docs",
            Name = "Documentation Site",
            Pages =
            [
                new PageTemplateDefinition
                {
                    Id = "home",
                    Route = "/",
                    Root = new LayoutNode
                    {
                        Children =
                        [
                            new ComponentNode
                            {
                                ComponentKey = "button",
                                Props =
                                {
                                    ["Text"] = new NodeProp
                                    {
                                        Name = "Text",
                                        Type = NodePropType.String,
                                        Value = "Hello from template"
                                    }
                                }
                            }
                        ]
                    }
                }
            ]
        };
}
