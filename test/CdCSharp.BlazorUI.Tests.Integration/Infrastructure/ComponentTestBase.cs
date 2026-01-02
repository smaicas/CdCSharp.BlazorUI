using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public abstract class ComponentTestBase : IDisposable
{
    protected BlazorTestContextBase Context { get; }

    protected ComponentTestBase(BlazorTestContextBase context)
    {
        Context = context;
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
