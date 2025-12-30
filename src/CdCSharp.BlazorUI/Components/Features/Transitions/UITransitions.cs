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

    public string GetDataAttributeValue()
    {
        return string.Join(" ",
            _transitions.SelectMany(t =>
                t.Value.Select(cfg =>
                    $"ui-transition-{t.Key.ToString().ToLower()}-{cfg.Type.ToString().ToLower()}"
                )));
    }

    public Dictionary<string, string> GetCssVariables()
    {
        Dictionary<string, string> variables = [];

        foreach ((TransitionTrigger trigger, List<TransitionConfig> configs) in _transitions)
        {
            foreach (TransitionConfig config in configs)
            {
                string prefix = $"--ui-transition-{trigger.ToString().ToLower()}";

                if (config.Duration.HasValue)
                    variables[$"{prefix}-duration"] =
                        $"{config.Duration.Value.TotalMilliseconds}ms";

                if (config.Delay.HasValue)
                    variables[$"{prefix}-delay"] =
                        $"{config.Delay.Value.TotalMilliseconds}ms";

                if (!string.IsNullOrEmpty(config.Easing))
                    variables[$"{prefix}-easing"] = config.Easing;

                foreach (KeyValuePair<string, string> prop in config.CustomProperties)
                    variables[$"{prefix}-{prop.Key}"] = prop.Value;
            }
        }

        return variables;
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

    // Background
    Background,

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