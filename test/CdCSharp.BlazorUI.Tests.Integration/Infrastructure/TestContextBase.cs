using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public class TestContextBase : BunitContext, IAsyncLifetime
{
    protected TestContextBase()
    {
        Services.AddBlazorUI();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    //public new void Dispose()
    //{
    //    base.Dispose();
    //    GC.SuppressFinalize(this);
    //}

    //public new async ValueTask DisposeAsync()
    //{
    //    await base.DisposeAsync();
    //    GC.SuppressFinalize(this);
    //}
}