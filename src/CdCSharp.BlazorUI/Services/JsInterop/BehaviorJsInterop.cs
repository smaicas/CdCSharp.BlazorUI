using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Javascript;

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