using CdCSharp.BlazorUI.Core.Theming.Css;

namespace CdCSharp.BlazorUI.Core.Transitions;

public class UITransitions
{
    private readonly Dictionary<TransitionTrigger, TransitionConfig> _transitions = [];

    public IReadOnlyDictionary<TransitionTrigger, TransitionConfig> Transitions => _transitions;

    public bool HasTransitions => _transitions.Any();

    internal void AddTransition(TransitionTrigger trigger, TransitionConfig config)
    {
        _transitions[trigger] = config;
    }

    public string GetCssClasses()
    {
        return string.Join(" ", _transitions.Select(t =>
            CssClassesReference.Transition(t.Key, t.Value.Type)));
    }

    public Dictionary<string, string> GetInlineStyles()
    {
        Dictionary<string, string> styles = [];

        foreach ((TransitionTrigger trigger, TransitionConfig? config) in _transitions)
        {
            if (config.Duration.HasValue)
            {
                styles[$"--ui-transition-{trigger.ToString().ToLower()}-duration"] =
                    $"{config.Duration.Value.TotalMilliseconds}ms";
            }

            if (config.Delay.HasValue)
            {
                styles[$"--ui-transition-{trigger.ToString().ToLower()}-delay"] =
                    $"{config.Delay.Value.TotalMilliseconds}ms";
            }

            if (!string.IsNullOrEmpty(config.Easing))
            {
                styles[$"--ui-transition-{trigger.ToString().ToLower()}-easing"] = config.Easing;
            }

            // Custom properties for specific transitions
            foreach (KeyValuePair<string, string> prop in config.CustomProperties)
            {
                styles[$"--ui-transition-{trigger.ToString().ToLower()}-{prop.Key}"] = prop.Value;
            }
        }

        return styles;
    }
}

public class TransitionConfig
{
    public TransitionType Type { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan? Delay { get; set; }
    public string? Easing { get; set; }
    public Dictionary<string, string> CustomProperties { get; set; } = [];
}

public enum TransitionTrigger
{
    Hover,
    Focus,
    Active,
    Disabled
}
public enum TransitionType
{
    // Transform
    Scale,
    Rotate,
    Translate,
    Skew,

    // Appearance
    Fade,
    Blur,
    Brightness,
    Contrast,
    Saturate,

    // Layout
    Shadow,
    Border,

    // Modern CSS
    BackdropBlur,

    // Combinations
    Lift,
    Glow,
    Pulse,
    Shake
}
