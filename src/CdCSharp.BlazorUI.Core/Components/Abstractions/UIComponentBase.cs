// Core/Components/Abstractions/UIComponentBase.cs
using CdCSharp.BlazorUI.Core.Effects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase
{
    private Dictionary<string, object>? _originalUserAttributes;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    [Parameter] public SimpleUIEffects? Effects { get; set; }

    // Unique ID for this component instance
    protected string ComponentId => field ??= $"ui-{Guid.NewGuid():N}".Substring(0, 8);

    public string ComputedCssClasses { get; private set; } = string.Empty;

    private string? _effectStyles;

    public virtual IEnumerable<string> GetAdditionalCssClasses() => [];
    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        // Store original user attributes on first run
        if (_originalUserAttributes == null)
        {
            _originalUserAttributes = new Dictionary<string, object>(AdditionalAttributes);
        }

        // Start fresh with original user attributes
        Dictionary<string, object> attributes = new(_originalUserAttributes);

        // Extract original user classes and styles
        string userClasses = _originalUserAttributes.TryGetValue("class", out object? origClass)
            ? origClass.ToString() ?? string.Empty
            : string.Empty;

        string userStyles = _originalUserAttributes.TryGetValue("style", out object? origStyle)
            ? origStyle.ToString() ?? string.Empty
            : string.Empty;

        // Build component classes (without user classes)
        List<string> componentClasses = [.. GetAdditionalCssClasses()];

        if (Effects?.HasEffects == true)
        {
            componentClasses.Add("ui-has-effects");
            attributes["data-effect-id"] = ComponentId;
            _effectStyles = Effects.GenerateStyles(ComponentId);
        }
        else
        {
            _effectStyles = null;
            attributes.Remove("data-effect-id");
        }

        // Combine component and user classes
        List<string> allClasses = componentClasses.ToList();
        if (!string.IsNullOrWhiteSpace(userClasses))
        {
            allClasses.Add(userClasses);
        }

        ComputedCssClasses = string.Join(" ", allClasses.Distinct()); // Distinct to avoid any duplicates

        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            attributes["class"] = ComputedCssClasses;
        }
        else
        {
            attributes.Remove("class");
        }

        // Build component styles (without user styles)
        Dictionary<string, string> componentStyles = GetAdditionalInlineStyles();

        // Combine styles
        List<string> allStyleParts = [];

        if (componentStyles.Any())
        {
            allStyleParts.AddRange(componentStyles.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        if (!string.IsNullOrWhiteSpace(userStyles))
        {
            allStyleParts.Add(userStyles);
        }

        string finalStyles = string.Join("; ", allStyleParts);

        if (!string.IsNullOrWhiteSpace(finalStyles))
        {
            attributes["style"] = finalStyles;
        }
        else
        {
            attributes.Remove("style");
        }

        // Replace AdditionalAttributes with clean version
        AdditionalAttributes = attributes;

        base.OnParametersSet();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Render effect styles if any
        if (!string.IsNullOrEmpty(_effectStyles))
        {
            builder.OpenElement(0, "style");
            builder.AddAttribute(1, "data-effect-styles", ComponentId);
            builder.AddContent(2, _effectStyles);
            builder.CloseElement();
        }

        // Let derived components render their content
        base.BuildRenderTree(builder);
    }
}