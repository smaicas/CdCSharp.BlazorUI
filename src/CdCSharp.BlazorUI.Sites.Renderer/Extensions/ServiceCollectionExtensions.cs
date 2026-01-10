using CdCSharp.BlazorUI.Sites.Renderer.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Sites.Renderer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUiSitesRenderer(
        this IServiceCollection services,
        Action<BlazorComponentRegistry>? configure = null)
    {
        BlazorComponentRegistry registry = new();
        configure?.Invoke(registry);

        services.AddSingleton<IBlazorComponentRegistry>(registry);

        return services;
    }
}
