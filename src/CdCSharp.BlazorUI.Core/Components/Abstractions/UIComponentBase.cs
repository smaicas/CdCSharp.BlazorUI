// Core/Components/Abstractions/UIComponentBase.cs
using CdCSharp.BlazorUI.Core.Theming.Css;
using CdCSharp.BlazorUI.Core.Transitions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    public string ComputedCssClasses { get; private set; } = string.Empty;

    public virtual IEnumerable<string> GetAdditionalCssClasses() => [];
    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        // Get component classes
        List<string> componentClasses = [.. GetAdditionalCssClasses()];

        // Check if component implements IHasTransitions
        if (this is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            componentClasses.Add(CssClassesReference.HasTransitions);
            foreach (string cssClass in hasTransitions.Transitions.GetCssClasses().Split(' '))
            {
                componentClasses.Add(cssClass);
            }
        }

        // Get user classes
        string userClasses = AdditionalAttributes.TryGetValue("class", out object? existingClass)
            ? existingClass.ToString() ?? string.Empty
            : string.Empty;

        // Combine all classes
        ComputedCssClasses = string.IsNullOrWhiteSpace(userClasses)
            ? string.Join(" ", componentClasses)
            : $"{string.Join(" ", componentClasses)} {userClasses}".Trim();

        // Update AdditionalAttributes with classes
        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            AdditionalAttributes["class"] = ComputedCssClasses;
        }

        // Build styles
        Dictionary<string, string> styles = GetAdditionalInlineStyles();

        // Add transition styles if applicable
        if (this is IHasTransitions hasTransitionsForStyles && hasTransitionsForStyles.Transitions?.HasTransitions == true)
        {
            foreach ((string? key, string? value) in hasTransitionsForStyles.Transitions.GetInlineStyles())
            {
                styles[key] = value;
            }
        }

        // Merge styles
        MergeAttribute("style", string.Join(";", styles.Select(kv => $"{kv.Key}: {kv.Value}")), ";");

        base.OnParametersSet();
    }

    private void MergeAttribute(string key, string newValue, string separator)
    {
        if (string.IsNullOrWhiteSpace(newValue)) { return; }

        if (AdditionalAttributes.TryGetValue(key, out object? existing))
        {
            AdditionalAttributes[key] = $"{newValue}{separator}{existing}";
        }
        else
        {
            AdditionalAttributes[key] = newValue;
        }
    }
}