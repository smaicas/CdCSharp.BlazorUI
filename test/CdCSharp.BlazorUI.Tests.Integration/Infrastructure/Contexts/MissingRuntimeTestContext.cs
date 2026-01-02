using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

public sealed class MissingRuntimeTestContext : BlazorTestContextBase
{
    public override string Scenario => "MissingRuntime";

    protected override void ConfigureScenarioServices(IServiceCollection services)
    {
        // IBlazorRuntime not registered
    }
}