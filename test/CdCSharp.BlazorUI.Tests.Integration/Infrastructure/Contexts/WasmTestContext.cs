using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

public sealed class WasmTestContext : BlazorTestContextBase
{
    public override string Scenario => "Wasm";

    protected override void ConfigureScenarioServices(IServiceCollection services)
    {
        services.AddBlazorUILocalizationWasm();

        //services.AddSingleton<ITestHostingModel>(new WasmHostingModel());

        // Localization WASM
        //services.AddSingleton<ITestCultureService, FakeWasmCultureService>();
    }
}