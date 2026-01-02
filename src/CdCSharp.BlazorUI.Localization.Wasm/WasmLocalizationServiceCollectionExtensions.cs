using CdCSharp.BlazorUI.Localization.Abstractions;
using CdCSharp.BlazorUI.Localization.Wasm;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

namespace Microsoft.Extensions.DependencyInjection;

public static class WasmLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUILocalization(
        this IServiceCollection services,
        Action<LocalizationSettings>? configure = null)
    {
        LocalizationSettings options = new();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Add standard localization
        services.AddLocalization(opts => opts.ResourcesPath = options.ResourcesPath);

        // Add WASM-specific persistence
        services.AddScoped<ILocalizationPersistence, WasmLocalizationPersistence>();

        return services;
    }
}

public static class WasmLocalizationHostExtensions
{
    public static async Task<WebAssemblyHost> UseBlazorUILocalization(
        this WebAssemblyHost host,
        string defaultCulture = "en-US")
    {
        IJSRuntime js = host.Services.GetRequiredService<IJSRuntime>();
        try
        {
            // Using eval to access localStorage directly
            string? storedCulture = await js.InvokeAsync<string?>("eval", $"window.localStorage.getItem('{WasmLocalizationPersistence.CULTURE_KEY}')");

            if (!string.IsNullOrEmpty(storedCulture))
            {
                CultureInfo culture = new(storedCulture);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            else
            {
                CultureInfo culture = new(defaultCulture);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                await js.InvokeVoidAsync("eval", $"window.localStorage.setItem('{WasmLocalizationPersistence.CULTURE_KEY}', '{defaultCulture}')");
            }
        }
        catch
        {
            CultureInfo culture = new(defaultCulture);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        return host;
    }
}
