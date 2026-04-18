using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Core.Abstractions.Services;
using CdCSharp.BlazorUI.Localization.Wasm;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Extensions;

public class FakeJsRuntime : IJSRuntime
{
    ValueTask<TValue> IJSRuntime.InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args) => throw new NotImplementedException();

    ValueTask<TValue> IJSRuntime.InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new NotImplementedException();
}

[Trait("Library", "Service Registration")]
public class ServiceRegistrationTests : BunitContext
{
    private static readonly Type[] ExpectedServiceTypes =
    {
        typeof(IVariantRegistry),
        typeof(IThemeJsInterop),
        typeof(IBehaviorJsInterop),
        typeof(IMemoryCache),
    };

    [Fact(DisplayName = "AddBlazorUI_RegistersAndResolvesAllServices")]
    public async Task AddBlazorUI_RegistersAndResolvesAllServices()
    {
        ServiceCollection services = new();
        services.AddScoped<IJSRuntime, FakeJsRuntime>();

        services.AddBlazorUI();
        await using ServiceProvider provider = services.BuildServiceProvider();

        AssertServicesAreRegistered(services);
        AssertServicesCanBeResolved(provider);
    }

    [Fact(DisplayName = "AddBlazorUIVariants_RegistersCustomVariants")]
    public void ServiceCollectionExtensions_AddBlazorUIVariants_RegistersCustomVariants()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddBlazorUI();
        TestVariant customVariant = TestVariant.Custom("Test");
        bool templateCalled = false;

        // Act
        services.AddBlazorUIVariants(builder =>
        {
            builder.ForComponent<TestVariantComponent>()
                .AddVariant(customVariant, _ =>
                {
                    templateCalled = true;
                    return __builder => { };
                });
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IVariantRegistry registry = provider.GetRequiredService<IVariantRegistry>();
        RenderFragment? template = registry.GetTemplate(typeof(TestVariantComponent), customVariant, null!);
        template?.Invoke(null!);

        // Assert
        templateCalled.Should().BeTrue();
    }

    private static void AssertServicesAreRegistered(
            IServiceCollection services)
    {
        foreach (Type serviceType in ExpectedServiceTypes)
        {
            services.Should().Contain(d => d.ServiceType == serviceType,
                $"Service {serviceType.Name} should be registered");
        }
    }

    private static void AssertServicesCanBeResolved(
    IServiceProvider provider)
    {
        foreach (Type serviceType in ExpectedServiceTypes)
        {
            provider.GetService(serviceType)
                .Should().NotBeNull($"{serviceType.Name} should be resolvable");
        }
    }

    // LIB-02: Localization registration tests

    [Fact(DisplayName = "AddBlazorUILocalizationServer_RegistersLocalizationSettings")]
    public void AddBlazorUILocalizationServer_RegistersLocalizationSettings()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazorUILocalizationServer(opts =>
        {
            opts.DefaultCulture = "es-ES";
            opts.CultureCookieName = ".Test.Culture";
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        CdCSharp.BlazorUI.Localization.Server.LocalizationSettings? settings =
            provider.GetService<CdCSharp.BlazorUI.Localization.Server.LocalizationSettings>();
        settings.Should().NotBeNull();
        settings!.DefaultCulture.Should().Be("es-ES");
        settings.CultureCookieName.Should().Be(".Test.Culture");
    }

[Fact(DisplayName = "AddBlazorUILocalizationWasm_RegistersLocalizationSettings")]
    public void AddBlazorUILocalizationWasm_RegistersLocalizationSettings()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazorUILocalizationWasm(opts =>
        {
            opts.DefaultCulture = "de-DE";
        });
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        CdCSharp.BlazorUI.Localization.Wasm.LocalizationSettings? settings =
            provider.GetService<CdCSharp.BlazorUI.Localization.Wasm.LocalizationSettings>();
        settings.Should().NotBeNull();
        settings!.DefaultCulture.Should().Be("de-DE");
    }

    [Fact(DisplayName = "AddBlazorUILocalizationWasm_RegistersILocalizationPersistence")]
    public void AddBlazorUILocalizationWasm_RegistersILocalizationPersistence()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddSingleton<IJSRuntime, FakeJsRuntime>();

        // Act
        services.AddBlazorUILocalizationWasm();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        ILocalizationPersistence? persistence = provider.GetService<ILocalizationPersistence>();
        persistence.Should().NotBeNull();
    }
}