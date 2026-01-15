using System.Globalization;

namespace CdCSharp.BlazorUI.Components;

public class BUITransitionsBuilder
{
    internal BUITransitions _transitions = new();

    public BUITransitions Build() => _transitions;

    public TriggerTransitionBuilder OnActive() => new(_transitions, TransitionTrigger.Active);

    public TriggerTransitionBuilder OnDisabled() => new(_transitions, TransitionTrigger.Disabled);

    public TriggerTransitionBuilder OnFocus() => new(_transitions, TransitionTrigger.Focus);

    public TriggerTransitionBuilder OnHover() => new(_transitions, TransitionTrigger.Hover);
}

public class TriggerTransitionBuilder
{
    private readonly BUITransitions _transitions;
    private readonly TransitionTrigger _trigger;

    internal TriggerTransitionBuilder(BUITransitions transitions, TransitionTrigger trigger)
    {
        _transitions = transitions;
        _trigger = trigger;
    }

    // Builder chaining
    public BUITransitionsBuilder And() => new() { _transitions = _transitions };

    public TriggerTransitionBuilder BackdropBlur(string amount = "8px", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.BackdropBlur, options);
        config.CustomProperties["amount"] = amount;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Background(string background = "rgba(0,0,0,0)", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Background, options);
        config.CustomProperties["background"] = background;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Blur(string amount = "2px", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Blur, options);
        config.CustomProperties["amount"] = amount;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Border(string border = "1px solid #cccccc", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Border, options);
        config.CustomProperties["border"] = border;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    // Add Build method to TriggerTransitionBuilder
    public BUITransitions Build() => _transitions;

    public TriggerTransitionBuilder Fade(float opacity = 0.7f, Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Fade, options);
        config.CustomProperties["opacity"] = opacity.ToString(CultureInfo.InvariantCulture);
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Glow(string color = "rgba(59, 130, 246, 0.5)", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Glow, options);
        config.CustomProperties["color"] = color;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    // Combined effects
    public TriggerTransitionBuilder Lift(Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Lift, options);
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Rotate(string angle = "5deg", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Rotate, options);
        config.CustomProperties["angle"] = angle;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    // Basic transitions
    public TriggerTransitionBuilder Scale(float scale = 1.05f, Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Scale, options);
        config.CustomProperties["scale"] = scale.ToString(CultureInfo.InvariantCulture);
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Shadow(string shadow = "0 4px 8px rgba(0, 0, 0, 0.2)", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Shadow, options);
        config.CustomProperties["shadow"] = shadow;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    public TriggerTransitionBuilder Translate(string x = "0", string y = "-4px", Action<TransitionOptions>? options = null)
    {
        TransitionConfig config = CreateConfig(TransitionType.Translate, options);
        config.CustomProperties["x"] = x;
        config.CustomProperties["y"] = y;
        _transitions.AddTransition(_trigger, config);
        return this;
    }

    private TransitionConfig CreateConfig(TransitionType type, Action<TransitionOptions>? options)
    {
        TransitionConfig config = new() { Type = type };
        TransitionOptions transitionOptions = new();
        options?.Invoke(transitionOptions);

        config.Duration = transitionOptions.Duration;
        config.Delay = transitionOptions.Delay;

        // Build easing if provided
        if (transitionOptions.Easing != null)
        {
            EasingBuilder easingBuilder = new();
            transitionOptions.Easing(easingBuilder);
            config.Easing = easingBuilder.Build();
        }

        return config;
    }
}

public class TransitionOptions
{
    public TimeSpan? Delay { get; set; }
    public TimeSpan? Duration { get; set; }
    public Action<EasingBuilder>? Easing { get; set; }
}