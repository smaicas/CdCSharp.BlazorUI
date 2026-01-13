using CdCSharp.BlazorUI.Localization.Wasm;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace Microsoft.Extensions.DependencyInjection;

public static class WasmLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUILocalizationWasm(
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
    public static async Task<WebAssemblyHost> UseBlazorUILocalizationWasm(
        this WebAssemblyHost host,
        string defaultCulture = "en-US")
    {
        ILocalizationPersistence locPersistence = host.Services.GetRequiredService<ILocalizationPersistence>();
        try
        {
            string? storedCulture = await locPersistence.GetStoredCultureAsync();

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

                await locPersistence.SetStoredCultureAsync(defaultCulture);
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