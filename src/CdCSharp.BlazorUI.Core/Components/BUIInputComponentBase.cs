using CdCSharp.BlazorUI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Abstractions;

// Base class without variants
public abstract class BUIInputComponentBase<TValue> :
    InputBase<TValue>,
    IAsyncDisposable,
    IBuiltComponent,
    IHasReadOnly,
    IHasDisabled,
    IHasRequired,
    IHasError
{
    // Shared style + JS-behavior pipeline. InputBase<TValue> is a mandatory ancestor for
    // EditContext/ValueExpression participation, so this class cannot inherit BUIComponentBase
    // directly. Composing the pipeline instead of re-implementing it keeps the two base classes
    // from drifting apart.
    private readonly BUIComponentPipeline _pipeline = new();
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _previousEditContext;
    private bool _lastValidationError;

    // Common parameters for all inputs — "force from outside": parent overrides the computed state.
    // The computed truth lives in IsX below. See CLAUDE.md §"State parameters".
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Error { get; set; }

    // Computed states — source of truth for gating, aria-* and the attributes builder.
    // IsDisabled is virtual so derived inputs can decouple Loading from Disabled — for
    // example, a debounced search input that wants to show a spinner while still
    // accepting keystrokes overrides this to drop the IHasLoading branch.
    public virtual bool IsDisabled => Disabled || (this is IHasLoading loading && loading.Loading);
    public bool IsError => Error || _lastValidationError;
    public bool IsReadOnly => ReadOnly;
    public bool IsRequired => Required;

    // See BUIComponentBase.ComputedAttributes for why this is `public`: variant templates live
    // cross-assembly and need to spread this dictionary onto the `<bui-component>` root.
    public Dictionary<string, object> ComputedAttributes => _pipeline.ComputedAttributes;

    /// <summary>
    /// `true` once <see cref="Dispose(bool)"/> / <see cref="DisposeAsync"/> has started. See
    /// BUIComponentBase.IsDisposed for the contract — gate post-await continuations in derived
    /// components on this flag.
    /// </summary>
    protected bool IsDisposed { get; set; }

    [Inject] private IBehaviorJsInterop BehaviorJsInterop { get; set; } = default!;

#if DEBUG
    [Inject] private IBUIPerformanceService? PerformanceService { get; set; }

    [Parameter]
    public bool TrackPerformanceEnabled { get; set; } = true;
#endif

    public override Task SetParametersAsync(ParameterView parameters)
    {
        bool hasValueExpression = false;
        bool hasEditContext = false;
        foreach (ParameterValue p in parameters)
        {
            if (p.Name == nameof(ValueExpression))
                hasValueExpression = true;
            else if (p.Cascading && p.Value is EditContext)
                hasEditContext = true;
        }

        if (hasValueExpression || hasEditContext)
            return base.SetParametersAsync(parameters);

        Dictionary<string, object?> patched = [];
        foreach (ParameterValue p in parameters)
            patched[p.Name] = p.Value;
        patched[nameof(ValueExpression)] = (Expression<Func<TValue>>)(() => Value!);
        return base.SetParametersAsync(ParameterView.FromDictionary(patched));
    }

    public virtual void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    { }

    public virtual void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    { }

    public virtual ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return _pipeline.DisposeBehaviorAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsDisposed = true;
            if (EditContext != null)
            {
                EditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }
        }

        base.Dispose(disposing);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
#if DEBUG
            _pipeline.EndInit(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
            if (IsDisposed) return;
            await _pipeline.AttachBehaviorAsync(this, BehaviorJsInterop);
            if (IsDisposed)
            {
                await _pipeline.DisposeBehaviorAsync();
                return;
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        _pipeline.BeginRenderTree();
        _pipeline.PatchVolatileAttributes(this);
        base.BuildRenderTree(builder);
#if DEBUG
        _pipeline.EndRenderTree(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    protected override void OnInitialized()
    {
        _pipeline.BeginInit();
        base.OnInitialized();

        if (ValueExpression != null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    protected override void OnParametersSet()
    {
        _pipeline.BeginParametersSet();
        _pipeline.BuildStyles(this, AdditionalAttributes);

        // Only re-subscribe if EditContext actually changed
        if (EditContext != null && ValueExpression != null)
        {
            if (_previousEditContext != EditContext)
            {
                // Unsubscribe from old context
                if (_previousEditContext != null)
                {
                    _previousEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
                }

                // Subscribe to new context
                EditContext.OnValidationStateChanged += HandleValidationStateChanged;
                _previousEditContext = EditContext;
            }

            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            _lastValidationError = EditContext.GetValidationMessages(_fieldIdentifier).Any();
        }
        else
        {
            _lastValidationError = false;
        }

        base.OnParametersSet();
#if DEBUG
        _pipeline.EndParametersSet(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        if (IsDisposed) return;
        bool current = EditContext != null && ValueExpression != null
            && EditContext.GetValidationMessages(_fieldIdentifier).Any();
        if (current != _lastValidationError)
        {
            _lastValidationError = current;
            // Full rebuild instead of PatchVolatileAttributes: Razor-generated BuildRenderTree on
            // derived .razor files does not call base.BuildRenderTree, so the patching hook is
            // bypassed. Rebuilding styles here (identical to the post-change path of OnParametersSet)
            // keeps data-bui-error in sync for the next render.
            _pipeline.BuildStyles(this, AdditionalAttributes);
            StateHasChanged();
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

    // Implementation of IVariantComponent interfaces
    Variant IVariantComponent.CurrentVariant => CurrentVariant;

    public TVariant CurrentVariant => Variant ?? DefaultVariant;
    public abstract TVariant DefaultVariant { get; }
    [Parameter] public TVariant? Variant { get; set; }

    Type IVariantComponent.VariantType => typeof(TVariant);
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    [Inject] private IVariantRegistry? VariantRegistry { get; set; }

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
