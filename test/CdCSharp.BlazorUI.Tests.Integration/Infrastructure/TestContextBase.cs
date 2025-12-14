using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public class TestContextBase : BunitContext, IDisposable
{
    protected TestContextBase()
    {
        Services.AddBlazorUI();
        // Common configuration
        //Services.AddSingleton<IVariantRegistry<UIButton, UIButtonVariant>>(new VariantRegistry<UIButton, UIButtonVariant>());

        // JSInterop configuration
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public new void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}