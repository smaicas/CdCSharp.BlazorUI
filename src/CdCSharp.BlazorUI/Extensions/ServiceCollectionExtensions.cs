using CdCSharp.BlazorUI.Core.Theming.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<IThemeService, ThemeService>();
        return services;
    }
}
