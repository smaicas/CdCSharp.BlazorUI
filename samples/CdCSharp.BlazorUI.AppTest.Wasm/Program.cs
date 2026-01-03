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

const string defaultCulture = "en-US";
IJSRuntime js = host.Services.GetRequiredService<IJSRuntime>();

//try
//{
//    // Intentar obtener la cultura guardada
//    string? storedCulture = await js.InvokeAsync<string?>("blazorCulture.get");

//    if (!string.IsNullOrEmpty(storedCulture))
//    {
//        CultureInfo culture = new(storedCulture);
//        CultureInfo.DefaultThreadCurrentCulture = culture;
//        CultureInfo.DefaultThreadCurrentUICulture = culture;
//    }
//    else
//    {
//        // Si no hay cultura guardada, usar la predeterminada
//        CultureInfo culture = new(defaultCulture);
//        CultureInfo.DefaultThreadCurrentCulture = culture;
//        CultureInfo.DefaultThreadCurrentUICulture = culture;

//        // Guardar la cultura predeterminada
//        await js.InvokeVoidAsync("blazorCulture.set", defaultCulture);
//    }
//}
//catch
//{
//    // En caso de error, usar la cultura predeterminada
//    CultureInfo culture = new(defaultCulture);
//    CultureInfo.DefaultThreadCurrentCulture = culture;
//    CultureInfo.DefaultThreadCurrentUICulture = culture;
//}

await host.UseBlazorUILocalizationWasm();

await host.RunAsync();