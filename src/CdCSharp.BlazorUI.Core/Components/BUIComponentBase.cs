using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Abstractions.Components;

public abstract class BUIComponentBase : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _behaviorInstance;
    private readonly BUIComponentAttributesBuilder _styleBuilder = new();

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private IBehaviorJsInterop BehaviorJsInterop { get; set; } = default!;

    // This is what components will use with @attributes
    public Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _styleBuilder.BuildStyles(this, AdditionalAttributes);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _behaviorInstance = await BUIComponentJsBehaviorBuilder
                .For(this, BehaviorJsInterop)
                .BuildAndAttachAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_behaviorInstance != null)
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
    }
}