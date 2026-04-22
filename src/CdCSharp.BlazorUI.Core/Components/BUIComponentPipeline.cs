using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Abstractions;

/// <summary>
/// Shared pipeline composed by every BUI component base class. Owns the
/// <see cref="BUIComponentAttributesBuilder"/> and the JS behavior handle so that
/// <see cref="BUIComponentBase"/> (which derives from <see cref="ComponentBase"/>) and
/// <see cref="BUIInputComponentBase{TValue}"/> (which must derive from
/// <c>InputBase&lt;TValue&gt;</c> for <c>EditContext</c> participation) share the same code path
/// instead of re-implementing the style+behavior lifecycle twice.
/// </summary>
internal sealed class BUIComponentPipeline
{
    private readonly BUIComponentAttributesBuilder _styleBuilder = new();
    private IJSObjectReference? _behaviorInstance;

    public Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
        => _styleBuilder.BuildStyles(component, additionalAttributes);

    public void PatchVolatileAttributes(ComponentBase component)
        => _styleBuilder.PatchVolatileAttributes(component);

    public async Task AttachBehaviorAsync(
        ComponentBase component,
        IBehaviorJsInterop behaviorJs)
    {
        _behaviorInstance = await BUIComponentJsBehaviorBuilder
            .For(component, behaviorJs)
            .BuildAndAttachAsync();
    }

    public async ValueTask DisposeBehaviorAsync()
    {
        if (_behaviorInstance == null) return;

        try
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server circuit disconnected — behavior already gone.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
        }
        catch (TaskCanceledException)
        {
            // Disposal raced with an in-flight call.
        }
    }
}
