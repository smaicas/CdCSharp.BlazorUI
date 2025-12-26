using Bunit;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Services;
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

[Trait("Extensions", "ServiceCollectionExtensions")]
public class ServiceCollectionExtensionsTests : BunitContext
{
    private static readonly Type[] ExpectedServiceTypes =
{
    typeof(IVariantRegistry<UIButton, UIButtonVariant>),
    typeof(IVariantRegistry<UISvgIcon, UISvgIconVariant>),
    typeof(IVariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>),
    typeof(IVariantRegistry<UILoadingIndicator, UILoadingIndicatorVariant>),
    typeof(IThemeJsInterop),
    typeof(IBehaviorJsInterop),
    typeof(IMemoryCache),
};

    [Fact(DisplayName = "AddBlazorUI_RegistersAllServices")]
    public void AddBlazorUI_RegistersAllServices()
    {
        ServiceCollection services = new();

        services.AddBlazorUI();

        AssertServicesAreRegistered(services);
    }

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
        UIButtonVariant customVariant = UIButtonVariant.Custom("Glass");
        bool templateCalled = false;

        // Act
        services.AddBlazorUIVariants(builder =>
        {
            builder.For<UIButton, UIButtonVariant>()
                .Register(customVariant, _ =>
                {
                    templateCalled = true;
                    return __builder => { };
                });
        });

        ServiceProvider provider = services.BuildServiceProvider();
        IVariantRegistry<UIButton, UIButtonVariant> registry = provider.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        RenderFragment? template = registry.GetTemplate(customVariant, null!);
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
}