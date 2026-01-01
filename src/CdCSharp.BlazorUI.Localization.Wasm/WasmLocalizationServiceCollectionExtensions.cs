using CdCSharp.BlazorUI.Localization.Abstractions;
using CdCSharp.BlazorUI.Localization.Wasm;

namespace Microsoft.Extensions.DependencyInjection;

public static class WasmLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddCdCSharpBlazorUILocalization(
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
