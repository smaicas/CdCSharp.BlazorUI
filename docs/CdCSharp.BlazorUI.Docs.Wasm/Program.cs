using CdCSharp.BlazorUI.Docs.Wasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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
        new CultureInfo("es-ES")
    ];
    options.DefaultCulture = "en-US";
});

builder.Services.AddBUIFluentValidation<Program>();
builder.Services.AddBUIFluentValidation();

WebAssemblyHost host = builder.Build();

await host.UseBlazorUILocalizationWasm();

await host.RunAsync();
