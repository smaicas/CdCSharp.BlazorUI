using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms.Abstractions;

// Base class without variants
public abstract class BUIInputComponentBase<TValue> : InputBase<TValue>, IAsyncDisposable
{
    private IJSObjectReference? _behaviorInstance;
    private readonly ComponentStyleBuilder _styleBuilder = new();
    private FieldIdentifier _fieldIdentifier;

    [Inject] private IBehaviorJsInterop? BehaviorJsInterop { get; set; }

    // Common parameters for all inputs
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }

    // Computed states - available for all inputs
    public bool IsError { get; private set; }
    public bool IsDisabled => Disabled || (this is IHasLoading loading && loading.IsLoading);
    public bool IsReadOnly => ReadOnly;
    public bool IsRequired => Required;

    // This is what components will use with @attributes
    protected Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ValueExpression != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    protected override void OnParametersSet()
    {
        // Build state attributes
        Dictionary<string, object> stateAttributes = new()
        {
            ["data-ui-error"] = IsError ? "true" : "false",
            ["data-ui-disabled"] = IsDisabled ? "true" : "false",
            ["data-ui-readonly"] = IsReadOnly ? "true" : "false",
            ["data-ui-required"] = IsRequired ? "true" : "false"
        };

        // Combine with AdditionalAttributes
        IReadOnlyDictionary<string, object> combinedAttributes =
            AdditionalAttributes != null
                ? stateAttributes.Concat(AdditionalAttributes).ToDictionary(x => x.Key, x => x.Value)
                : stateAttributes;

        _styleBuilder.BuildStyles(this, combinedAttributes);

        // Update validation state
        if (EditContext != null && ValueExpression != null)
        {
            IsError = EditContext.GetValidationMessages(_fieldIdentifier).Any();

            // Subscribe only once
            EditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            EditContext.OnValidationStateChanged += HandleValidationStateChanged;
        }

        base.OnParametersSet();
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        if (EditContext != null && ValueExpression != null)
        {
            bool hadErrors = IsError;
            IsError = EditContext.GetValidationMessages(_fieldIdentifier).Any();

            if (hadErrors != IsError)
            {
                // Rebuild attributes if state changed
                OnParametersSet();
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && BehaviorJsInterop != null && this is IJsBehavior jsBehavior)
        {
            // Check if component is loading - don't attach behaviors during loading state
            if (this is IHasLoading hasLoading && hasLoading.IsLoading)
            {
                return; // Skip behavior attachment when loading
            }

            ElementReference rootElement = jsBehavior.GetRootElement();
            BehaviorConfiguration config = new();

            // Configure ripple if applicable
            if (this is IHasRipple hasRipple && !hasRipple.DisableRipple)
            {
                config.Ripple = new RippleConfiguration
                {
                    Color = hasRipple.RippleColor?.ToString(ColorOutputFormats.Rgba),
                    Duration = hasRipple.RippleDuration
                };
            }

            // Attach behaviors if any configured
            if (config.HasAnyBehavior)
            {
                _behaviorInstance = await BehaviorJsInterop.AttachBehaviorsAsync(
                    rootElement, config);
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && EditContext != null)
        {
            EditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }

        base.Dispose(disposing);
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

// Base class with variants
public abstract class BUIInputComponentBase<TValue, TComponent, TVariant>
    : BUIInputComponentBase<TValue>, IVariantComponent<TVariant>
    where TComponent : BUIInputComponentBase<TValue, TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    private VariantHelper<TComponent, TVariant>? _variantHelper;

    [Parameter] public TVariant? Variant { get; set; }

    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    public abstract TVariant DefaultVariant { get; }

    [Inject] private IUniversalVariantRegistry? VariantRegistry { get; set; }

    // Implementation of IVariantComponent interfaces
    Variant IVariantComponent.CurrentVariant => CurrentVariant;
    Type IVariantComponent.VariantType => typeof(TVariant);
    public TVariant CurrentVariant => Variant ?? DefaultVariant;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Let the base class handle its render logic first
        base.BuildRenderTree(builder);

        // Then add the variant template
        if (_resolvedTemplate is not null)
        {
            builder.AddContent(0, _resolvedTemplate);
        }
    }

    protected override void OnParametersSet()
    {
        // First let the base class handle its parameter setting
        base.OnParametersSet();

        // Then handle variant resolution
        _variantHelper ??= new VariantHelper<TComponent, TVariant>(
            (TComponent)this,
            VariantRegistry);

        Variant ??= DefaultVariant;
        _resolvedTemplate = _variantHelper.ResolveTemplate(Variant, BuiltInTemplates);
    }
}