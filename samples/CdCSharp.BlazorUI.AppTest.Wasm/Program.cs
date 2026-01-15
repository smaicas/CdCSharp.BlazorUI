using CdCSharp.BlazorUI.AppTest.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorUI();
builder.Services.AddBlazorUILocalizationWasm(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr-FR")
    ];
    options.DefaultCulture = "en-US";
});

WebAssemblyHost host = builder.Build();

IJSRuntime js = host.Services.GetRequiredService<IJSRuntime>();

await host.UseBlazorUILocalizationWasm();

await host.RunAsync();