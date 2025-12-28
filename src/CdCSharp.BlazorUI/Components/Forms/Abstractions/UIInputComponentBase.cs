using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Css;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms.Abstractions;

public abstract class UIInputComponentBase<TValue> : InputBase<TValue>,
    IAsyncDisposable,
    IHasSize,
    IHasFullWidth,
    IHasLoading,
    IHasDensity,
    IHasTransitions,
    IHasElevation,
    IHasBorder
{
    private IJSObjectReference? _behaviorInstance;
    protected string ComputedCssClasses { get; private set; } = string.Empty;
    protected Dictionary<string, object> ComputedAttributes { get; private set; } = [];

    [Inject] private IBehaviorJsInterop? BehaviorJsInterop { get; set; }

    // IHasSize
    [Parameter] public SizeEnum Size { get; set; } = SizeEnum.Medium;

    // IHasFullWidth
    [Parameter] public bool FullWidth { get; set; }

    // IHasLoading
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public UILoadingIndicatorVariant? LoadingIndicatorVariant { get; set; }

    // IHasDensity
    [Parameter] public DensityEnum Density { get; set; } = DensityEnum.Standard;

    // IHasTransitions
    [Parameter] public UITransitions? Transitions { get; set; }

    // IHasElevation
    [Parameter] public int? Elevation { get; set; }

    // IHasBorder
    [Parameter] public BorderStyle? Border { get; set; }
    [Parameter] public BorderStyle? BorderTop { get; set; }
    [Parameter] public BorderStyle? BorderRight { get; set; }
    [Parameter] public BorderStyle? BorderBottom { get; set; }
    [Parameter] public BorderStyle? BorderLeft { get; set; }

    // Additional common parameters
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? HelperText { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public InputVariant Variant { get; set; } = InputVariant.Outlined;

    protected bool IsDisabled => Disabled || IsLoading;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        UpdateComputedProperties();
    }

    protected virtual void UpdateComputedProperties()
    {
        // Build CSS classes
        List<string> cssClasses = [
            CssClass,  // Original CSS class from InputBase
            "ui-input",
            $"ui-input--{Variant.ToString().ToLowerInvariant()}",
            CssClassesReference.Size(Size)
        ];

        if (FullWidth) cssClasses.Add(CssClassesReference.FullWidth);
        if (IsLoading) cssClasses.Add(CssClassesReference.Loading);
        if (IsDisabled) cssClasses.Add("ui-input--disabled");
        if (ReadOnly) cssClasses.Add("ui-input--readonly");
        if (Required) cssClasses.Add("ui-input--required");

        if (Transitions?.HasTransitions == true)
        {
            cssClasses.Add(CssClassesReference.HasTransitions);
            cssClasses.AddRange(Transitions.GetCssClasses().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        if (Elevation.HasValue)
        {
            cssClasses.Add(CssClassesReference.Elevation(Math.Clamp(Elevation.Value, 0, 24)));
        }

        cssClasses.Add(CssClassesReference.Density(Density));
        cssClasses.AddRange(GetAdditionalCssClasses());

        ComputedCssClasses = string.Join(" ", cssClasses.Where(c => !string.IsNullOrWhiteSpace(c)));

        // Build attributes
        ComputedAttributes = new(AdditionalAttributes ?? new Dictionary<string, object>())
        {
            ["class"] = ComputedCssClasses,
            ["disabled"] = IsDisabled,
            ["readonly"] = ReadOnly,
            ["required"] = Required
        };

        if (!string.IsNullOrWhiteSpace(Placeholder))
        {
            ComputedAttributes["placeholder"] = Placeholder;
        }

        // Add inline styles
        Dictionary<string, string> styles = GetAdditionalInlineStyles();

        if (Transitions?.HasTransitions == true)
        {
            foreach ((string? key, string? value) in Transitions.GetInlineStyles())
            {
                styles[key] = value;
            }
        }

        if (Border != null || BorderTop != null || BorderRight != null || BorderBottom != null || BorderLeft != null)
        {
            AddBorderStyles(styles);
        }

        if (styles.Any())
        {
            ComputedAttributes["style"] = string.Join("; ", styles.Select(kv => $"{kv.Key}: {kv.Value}"));
        }
    }

    private void AddBorderStyles(Dictionary<string, string> styles)
    {
        if (Border?.Radius != null)
        {
            styles["border-radius"] = Border.GetRadiusCssValue();
        }

        if (Border != null)
            styles["border"] = Border.ToCssValue();

        if (BorderTop != null)
            styles["border-top"] = BorderTop.ToCssValue();

        if (BorderRight != null)
            styles["border-right"] = BorderRight.ToCssValue();

        if (BorderBottom != null)
            styles["border-bottom"] = BorderBottom.ToCssValue();

        if (BorderLeft != null)
            styles["border-left"] = BorderLeft.ToCssValue();
    }

    protected virtual IEnumerable<string> GetAdditionalCssClasses() => [];
    protected virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    public virtual async ValueTask DisposeAsync()
    {
        if (_behaviorInstance != null)
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
    }
}

public enum InputVariant
{
    Outlined,
    Filled,
    Standard
}
