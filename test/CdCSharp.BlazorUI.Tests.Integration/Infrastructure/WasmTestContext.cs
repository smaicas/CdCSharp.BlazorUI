using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public sealed class WasmTestContext : BlazorTestContextBase
{
    public override string Scenario => "Wasm";

    protected override void ConfigureScenarioServices(IServiceCollection services)
    {
        services.AddSingleton<ITestHostingModel>(new WasmHostingModel());

        // Localization WASM
        services.AddSingleton<ITestCultureService, FakeWasmCultureService>();
    }
}
