using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public class TestContextBase : BunitContext, IDisposable
{
    protected TestContextBase()
    {
        // Common configuration
        Services.AddSingleton<IVariantRegistry<UIButton, UIButtonVariant>>(new VariantRegistry<UIButton, UIButtonVariant>());

        // JSInterop configuration
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public new void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}