using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

public abstract class BlazorTestContextBase : BunitContext
{
    protected BlazorTestContextBase()
    {
        ConfigureCommonServices(Services);
        ConfigureScenarioServices(Services);
    }

    public abstract string Scenario { get; }

    protected virtual void ConfigureCommonServices(IServiceCollection services)
    {
        // === Registrar la librería REAL ===
        services.AddBlazorUI();

        // JSInterop fake (bUnit controla IJSRuntime)
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Navigation manager común
        services.AddSingleton<FakeNavigationManager>();
        services.AddSingleton<NavigationManager>(sp =>
        sp.GetRequiredService<FakeNavigationManager>());
    }

    protected abstract void ConfigureScenarioServices(IServiceCollection services);
}