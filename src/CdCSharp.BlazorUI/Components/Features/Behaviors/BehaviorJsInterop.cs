using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Features.Behaviors;

public interface IBehaviorJsInterop
{
    ValueTask<IJSObjectReference> AttachBehaviorsAsync(
        ElementReference element,
        BehaviorConfiguration configuration);
}

public sealed class BehaviorJsInterop : ModuleJsInteropBase, IBehaviorJsInterop
{
    public BehaviorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.BehaviorsJs)
    {
    }

    public async ValueTask<IJSObjectReference> AttachBehaviorsAsync(
        ElementReference element,
        BehaviorConfiguration configuration)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<IJSObjectReference>(
            "attachBehaviors", element, configuration);
    }
}

public class BehaviorConfiguration
{
    public RippleConfiguration? Ripple { get; set; }

    public bool HasAnyBehavior => Ripple != null;
}

public class RippleConfiguration
{
    public string? Color { get; set; }
    public int Duration { get; set; }
}
