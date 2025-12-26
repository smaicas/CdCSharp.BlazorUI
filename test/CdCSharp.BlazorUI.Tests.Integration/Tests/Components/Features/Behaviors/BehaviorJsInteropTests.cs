using Bunit;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Types;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Behaviors;

[Trait("JSInterop", "BehaviorJsInterop")]
public class BehaviorJsInteropTests : TestContextBase
{
    [Fact(DisplayName = "AttachBehaviorsAsync_CallsJSWithCorrectParameters")]
    public async Task BehaviorJsInterop_AttachBehaviorsAsync_CallsJSWithCorrectParameters()
    {
        // Arrange
        BunitJSModuleInterop moduleInterop = JSInterop.SetupModule(JSModulesReference.BehaviorsJs);
        moduleInterop.SetupVoid("attachBehaviors").SetVoidResult();

        BehaviorJsInterop behaviorInterop = new(JSInterop.JSRuntime);
        ElementReference elementRef = new();
        BehaviorConfiguration config = new()
        {
            Ripple = new RippleConfiguration
            {
                Color = "rgba(255,0,0,1)",
                Duration = 300
            }
        };

        // Act
        await behaviorInterop.AttachBehaviorsAsync(elementRef, config);

        // Assert
        IReadOnlyList<JSRuntimeInvocation> invocations = moduleInterop.Invocations["attachBehaviors"];
        invocations.Should().HaveCount(1);

        IReadOnlyList<object?> args = invocations.First().Arguments;
        args[0].Should().Be(elementRef);
        args[1].Should().BeOfType<BehaviorConfiguration>();

        BehaviorConfiguration passedConfig = args[1] as BehaviorConfiguration;
        passedConfig!.Ripple!.Color.Should().Be("rgba(255,0,0,1)");
        passedConfig.Ripple.Duration.Should().Be(300);
    }

    [Fact(DisplayName = "DisposeAsync_DisposesModule")]
    public async Task BehaviorJsInterop_DisposeAsync_DisposesModule()
    {
        // Arrange
        BunitJSModuleInterop moduleInterop = JSInterop.SetupModule(JSModulesReference.BehaviorsJs);
        BehaviorJsInterop behaviorInterop = new(JSInterop.JSRuntime);

        // Force module loading
        moduleInterop.SetupVoid("attachBehaviors").SetVoidResult();
        await behaviorInterop.AttachBehaviorsAsync(new ElementReference(), new BehaviorConfiguration());

        // Act
        await behaviorInterop.DisposeAsync();

        // Assert - Should not throw
        Func<Task> act = async () => await behaviorInterop.DisposeAsync();
        await act.Should().NotThrowAsync();
    }
}
