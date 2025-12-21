// Core/Components/Abstractions/UIComponentBase.cs
using CdCSharp.BlazorUI.Core.Effects;
using CdCSharp.BlazorUI.Core.Effects.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase, IDisposable
{
    private Dictionary<string, object>? _originalUserAttributes;
    private readonly Dictionary<EffectTrigger, Timer?> _effectTimers = [];
    private readonly HashSet<EffectTrigger> _activeEffects = [];

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    [Parameter] public SimpleUIEffects? Effects { get; set; }

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

        // Build component classes
        List<string> componentClasses = [.. GetAdditionalCssClasses()];

        if (Effects?.HasEffects == true)
        {
            componentClasses.Add("ui-has-effects");

            // Add active effect classes
            foreach (EffectTrigger activeTrigger in _activeEffects)
            {
                componentClasses.Add($"ui-effect-{activeTrigger.ToString().ToLower()}-active");
            }

            attributes["data-effect-id"] = ComponentId;
            _effectStyles = Effects.GenerateStyles(ComponentId);

            // Wire up JavaScript event handlers if needed
            if (Effects.RequiresJavaScript)
            {
                WireUpEventHandlers(attributes);
            }
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

        ComputedCssClasses = string.Join(" ", allClasses.Distinct());

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

    private void WireUpEventHandlers(Dictionary<string, object> attributes)
    {
        Dictionary<EffectTrigger, SimpleUIEffects.EffectDefinition> jsEffects = Effects!.GetJavaScriptEffects();

        foreach ((EffectTrigger trigger, SimpleUIEffects.EffectDefinition? effect) in jsEffects)
        {
            switch (trigger)
            {
                case EffectTrigger.Click:
                    WireUpClickHandler(attributes, effect);
                    break;
                case EffectTrigger.DoubleClick:
                    attributes["ondblclick"] = EventCallback.Factory.Create<MouseEventArgs>(this,
                        () => TriggerEffect(trigger, effect));
                    break;
                case EffectTrigger.MouseDown:
                    attributes["onmousedown"] = EventCallback.Factory.Create<MouseEventArgs>(this,
                        () => TriggerEffect(trigger, effect));
                    break;
                case EffectTrigger.MouseUp:
                    attributes["onmouseup"] = EventCallback.Factory.Create<MouseEventArgs>(this,
                        () => TriggerEffect(trigger, effect));
                    break;
            }
        }
    }

    private void WireUpClickHandler(Dictionary<string, object> attributes, SimpleUIEffects.EffectDefinition effect)
    {
        // Preserve original onclick if exists
        object? originalOnClick = _originalUserAttributes?.GetValueOrDefault("onclick");

        attributes["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, async (MouseEventArgs args) =>
        {
            // Trigger the effect
            TriggerEffect(EffectTrigger.Click, effect);

            // Call original handler if exists
            if (originalOnClick is EventCallback<MouseEventArgs> originalCallback)
            {
                await originalCallback.InvokeAsync(args);
            }
        });
    }

    private void TriggerEffect(EffectTrigger trigger, SimpleUIEffects.EffectDefinition effect)
    {
        // Cancel any existing timer for this trigger
        if (_effectTimers.TryGetValue(trigger, out Timer? existingTimer))
        {
            existingTimer?.Dispose();
        }

        // Add active class
        _activeEffects.Add(trigger);
        InvokeAsync(StateHasChanged);

        // Calculate total duration including delay
        TimeSpan totalDuration = effect.Duration + effect.Delay;
        if (effect.AnimationName != null && effect.IterationCount > 1)
        {
            totalDuration = TimeSpan.FromMilliseconds(totalDuration.TotalMilliseconds * effect.IterationCount);
        }

        // Remove class after animation completes
        _effectTimers[trigger] = new Timer(_ =>
        {
            _activeEffects.Remove(trigger);
            InvokeAsync(StateHasChanged);
        }, null, totalDuration, Timeout.InfiniteTimeSpan);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!string.IsNullOrEmpty(_effectStyles))
        {
            builder.OpenElement(0, "style");
            builder.AddAttribute(1, "data-effect-styles", ComponentId);
            builder.AddContent(2, _effectStyles);
            builder.CloseElement();
        }

        base.BuildRenderTree(builder);
    }

    public void Dispose()
    {
        foreach (Timer? timer in _effectTimers.Values)
        {
            timer?.Dispose();
        }
    }
}