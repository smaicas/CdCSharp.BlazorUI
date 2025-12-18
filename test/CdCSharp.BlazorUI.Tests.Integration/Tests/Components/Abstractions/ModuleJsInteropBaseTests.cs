using Bunit;
using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Abstractions", "ModuleJsInteropBase")]
public class ModuleJsInteropBaseTests : TestContextBase
{
    private class TestJsInterop : ModuleJsInteropBase
    {
        public TestJsInterop(IJSRuntime jsRuntime) : base(jsRuntime, "test-module.js")
        {
        }

        public Task<IJSObjectReference> GetModuleForTesting() => ModuleTask.Value;
        public bool IsModuleCreated => ModuleTask.IsValueCreated;
    }

    [Fact(DisplayName = "LazyLoadsModule")]
    public void ModuleJsInteropBase_LazyLoadsModule()
    {
        // Arrange
        TestJsInterop testInterop = new(JSInterop.JSRuntime);

        // Assert - Module not loaded yet
        testInterop.IsModuleCreated.Should().BeFalse();
    }

    [Fact(DisplayName = "LoadsModuleOnFirstAccess")]
    public async Task ModuleJsInteropBase_LoadsModuleOnFirstAccess()
    {
        // Arrange
        JSInterop.SetupModule("test-module.js");
        TestJsInterop testInterop = new(JSInterop.JSRuntime);

        // Act
        IJSObjectReference module = await testInterop.GetModuleForTesting();

        // Assert
        module.Should().NotBeNull();
        testInterop.IsModuleCreated.Should().BeTrue();
    }

    [Fact(DisplayName = "ThrowsWhenJSRuntimeIsNull")]
    public void ModuleJsInteropBase_ThrowsWhenJSRuntimeIsNull()
    {
        // Act & Assert
        Action act = () => new TestJsInterop(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("jsRuntime");
    }
}
