using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public sealed class ServerTestContext : BlazorTestContextBase
{
    public override string Scenario => "Server";

    protected override void ConfigureScenarioServices(IServiceCollection services)
    {
        services.AddSingleton<ITestHostingModel>(new ServerHostingModel());

        // Localization Server
        services.AddSingleton<ITestCultureService, FakeServerCultureService>();
    }
}