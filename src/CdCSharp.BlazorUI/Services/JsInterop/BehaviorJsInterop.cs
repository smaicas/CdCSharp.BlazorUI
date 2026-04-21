using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components;

internal sealed class BehaviorJsInterop : ModuleJsInteropBase, IBehaviorJsInterop
{
    public BehaviorJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.BehaviorsJs)
    {
    }

    public async ValueTask<IJSObjectReference> AttachBehaviorsAsync(BehaviorConfiguration configuration)
    {
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<IJSObjectReference>(
            "attachBehaviors", configuration);
    }
}