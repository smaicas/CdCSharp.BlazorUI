using CdCSharp.BlazorUI.Css;

namespace CdCSharp.BlazorUI.Components.Features.Transitions;

public enum TransitionTrigger
{
    Hover,
    Focus,
    Active,
    Disabled
}

public class TransitionConfig
{
    public Dictionary<string, string> CustomProperties { get; set; } = [];
    public TimeSpan? Delay { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Easing { get; set; }
    public TransitionType Type { get; set; }
}

public class UITransitions
{
    private readonly Dictionary<TransitionTrigger, List<TransitionConfig>> _transitions = [];

    public bool HasTransitions => _transitions.Any();

    public IReadOnlyDictionary<TransitionTrigger, IReadOnlyList<TransitionConfig>> Transitions =>
            _transitions.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<TransitionConfig>)kvp.Value
        );

    public string GetCssClasses()
    {
        return string.Join(" ",
            _transitions.SelectMany(t =>
                t.Value.Select(cfg =>
                    CssClassesReference.Transition(t.Key, cfg.Type)
                )));
    }

    public Dictionary<string, string> GetInlineStyles()
    {
        Dictionary<string, string> styles = [];

        foreach ((TransitionTrigger trigger, List<TransitionConfig> configs) in _transitions)
        {
            foreach (TransitionConfig config in configs)
            {
                string prefix = $"--ui-transition-{trigger.ToString().ToLower()}";

                if (config.Duration.HasValue)
                    styles[$"{prefix}-duration"] =
                        $"{config.Duration.Value.TotalMilliseconds}ms";

                if (config.Delay.HasValue)
                    styles[$"{prefix}-delay"] =
                        $"{config.Delay.Value.TotalMilliseconds}ms";

                if (!string.IsNullOrEmpty(config.Easing))
                    styles[$"{prefix}-easing"] = config.Easing;

                foreach (KeyValuePair<string, string> prop in config.CustomProperties)
                    styles[$"{prefix}-{prop.Key}"] = prop.Value;
            }
        }

        return styles;
    }

    internal void AddTransition(TransitionTrigger trigger, TransitionConfig config)
    {
        if (!_transitions.TryGetValue(trigger, out List<TransitionConfig>? list))
        {
            list = [];
            _transitions[trigger] = list;
        }

        list.Add(config);
    }
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