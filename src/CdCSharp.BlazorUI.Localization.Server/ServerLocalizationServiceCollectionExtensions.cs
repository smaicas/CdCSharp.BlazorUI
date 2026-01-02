using CdCSharp.BlazorUI.Localization.Abstractions;
using CdCSharp.BlazorUI.Localization.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServerLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUILocalizationServer(
        this IServiceCollection services,
        Action<LocalizationSettings>? configure = null)
    {
        LocalizationSettings options = new();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Add Runtime detector
        services.AddSingleton<IBlazorRuntime, ServerBlazorRuntime>();

        // Add standard localization
        services.AddLocalization(opts => opts.ResourcesPath = options.ResourcesPath);

        // Add Server-specific services
        services.AddHttpContextAccessor();
        services.AddScoped<ILocalizationPersistence, ServerLocalizationPersistence>();

        // Configure request localization
        services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.DefaultRequestCulture = new RequestCulture(options.DefaultCulture);
            opts.SupportedCultures = options.SupportedCultures;
            opts.SupportedUICultures = options.SupportedCultures;

            // Cookie provider should be first
            opts.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
            {
                CookieName = options.CultureCookieName
            });
        });

        return services;
    }
}

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBlazorUILocalizationServer(
        this IApplicationBuilder app)
    {
        return app.UseRequestLocalization();
    }
}
