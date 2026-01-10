using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Sites.AppTest.Wasm;
using CdCSharp.BlazorUI.Sites.AppTest.Wasm.Templates;
using CdCSharp.BlazorUI.Sites.Core;
using CdCSharp.BlazorUI.Sites.Core.Templates;
using CdCSharp.BlazorUI.Sites.Renderer.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorUI();
builder.Services.AddBlazorUiSitesRenderer(registry =>
{
    registry.Register<BUIButton>(
        "button",
        new ComponentMetadata
        {
            Key = "button",
            DisplayName = "Button",
            Category = "Basic",
            Props =
            [
                new() { Name = "Text", Type = NodePropType.String }
            ]
        });
});

SiteTemplateDefinition template = DocsTemplate.Create();

SiteTemplateInstantiator instantiator = new();

SiteDefinition site = instantiator.CreateSite(
    template,
    siteId: "example-site",
    siteName: "Example WASM Site");

builder.Services.AddSingleton(site);

await builder.Build().RunAsync();
