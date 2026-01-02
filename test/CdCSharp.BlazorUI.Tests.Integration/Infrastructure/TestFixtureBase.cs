using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public abstract class TestFixtureBase<TContext> : IClassFixture<TContext>
    where TContext : BlazorTestContextBase
{
    protected TContext Context { get; }

    protected TestFixtureBase(TContext context)
    {
        Context = context;
    }
}
