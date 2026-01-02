namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public class TestScenarios
{
    public static IEnumerable<object[]> All =>
        [
            new object[] { new ServerTestContext() },
            new object[] { new WasmTestContext() },
        ];
}