using CdCSharp.BlazorUI.Core.Effects.Builders;
using System.Text;

namespace CdCSharp.BlazorUI.Core.Effects;

public class SimpleUIEffects
{
    private readonly Dictionary<EffectTrigger, EffectDefinition> _effects = [];

    public class EffectDefinition
    {
        public Dictionary<string, string> Properties { get; set; } = [];
        public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(300);
        public string Easing { get; set; } = "ease-in-out";
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
        public string? AnimationName { get; set; }
        public int IterationCount { get; set; } = 1;
        public string FillMode { get; set; } = "forwards";
        public bool RequiresJavaScript { get; set; } = false;
    }

    public bool HasEffects => _effects.Any();
    public bool RequiresJavaScript => _effects.Any(e => e.Value.RequiresJavaScript || IsJavaScriptTrigger(e.Key));

    public void AddEffect(EffectTrigger trigger, Action<EffectDefinition> configure)
    {
        EffectDefinition definition = new();
        configure(definition);

        // Auto-detect if JS is required
        if (IsJavaScriptTrigger(trigger))
        {
            definition.RequiresJavaScript = true;
        }

        _effects[trigger] = definition;
    }

    // Get effects that need JavaScript handling
    public Dictionary<EffectTrigger, EffectDefinition> GetJavaScriptEffects()
    {
        return _effects
            .Where(e => e.Value.RequiresJavaScript || IsJavaScriptTrigger(e.Key))
            .ToDictionary(e => e.Key, e => e.Value);
    }

    // Generate CSS for pure CSS effects only
    public string GenerateStyles(string componentId)
    {
        if (!_effects.Any()) return string.Empty;

        StringBuilder sb = new();

        // Only generate CSS for non-JS triggers
        IEnumerable<KeyValuePair<EffectTrigger, EffectDefinition>> cssEffects = _effects.Where(e => !e.Value.RequiresJavaScript && !IsJavaScriptTrigger(e.Key));

        foreach ((EffectTrigger trigger, EffectDefinition? effect) in cssEffects)
        {
            string selector = $"[data-effect-id='{componentId}']{GetTriggerSelector(trigger)}";

            sb.AppendLine($"{selector} {{");

            foreach (KeyValuePair<string, string> prop in effect.Properties)
            {
                sb.AppendLine($"  {prop.Key}: {prop.Value};");
            }

            if (effect.AnimationName != null)
            {
                sb.AppendLine($"  animation-name: {effect.AnimationName};");
                sb.AppendLine($"  animation-duration: {effect.Duration.TotalMilliseconds}ms;");
                sb.AppendLine($"  animation-timing-function: {effect.Easing};");
                sb.AppendLine($"  animation-delay: {effect.Delay.TotalMilliseconds}ms;");
                sb.AppendLine($"  animation-iteration-count: {(effect.IterationCount == -1 ? "infinite" : effect.IterationCount.ToString())};");
                sb.AppendLine($"  animation-fill-mode: {effect.FillMode};");
            }
            else
            {
                sb.AppendLine($"  transition-duration: {effect.Duration.TotalMilliseconds}ms;");
                sb.AppendLine($"  transition-timing-function: {effect.Easing};");
                sb.AppendLine($"  transition-delay: {effect.Delay.TotalMilliseconds}ms;");
            }

            sb.AppendLine("}");
        }

        // Add CSS classes for JS-triggered effects
        foreach ((EffectTrigger trigger, EffectDefinition? effect) in GetJavaScriptEffects())
        {
            string className = $".ui-effect-{trigger.ToString().ToLower()}-active";
            sb.AppendLine($"[data-effect-id='{componentId}']{className} {{");

            foreach (KeyValuePair<string, string> prop in effect.Properties)
            {
                sb.AppendLine($"  {prop.Key}: {prop.Value} !important;");
            }

            if (effect.AnimationName != null)
            {
                sb.AppendLine($"  animation-name: {effect.AnimationName} !important;");
                sb.AppendLine($"  animation-duration: {effect.Duration.TotalMilliseconds}ms !important;");
                sb.AppendLine($"  animation-timing-function: {effect.Easing} !important;");
                sb.AppendLine($"  animation-delay: {effect.Delay.TotalMilliseconds}ms !important;");
                sb.AppendLine($"  animation-iteration-count: {(effect.IterationCount == -1 ? "infinite" : effect.IterationCount.ToString())} !important;");
                sb.AppendLine($"  animation-fill-mode: {effect.FillMode} !important;");
            }

            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private bool IsJavaScriptTrigger(EffectTrigger trigger) => trigger switch
    {
        EffectTrigger.Click => true,
        EffectTrigger.DoubleClick => true,
        EffectTrigger.MouseDown => true,
        EffectTrigger.MouseUp => true,
        EffectTrigger.TouchStart => true,
        EffectTrigger.TouchEnd => true,
        EffectTrigger.Appear => true,
        EffectTrigger.Disappear => true,
        _ => false
    };

    private string GetTriggerSelector(EffectTrigger trigger) => trigger switch
    {
        EffectTrigger.Hover => ":hover",
        EffectTrigger.Focus => ":focus",
        EffectTrigger.Active => ":active",
        EffectTrigger.Disabled => ":disabled",
        _ => ""
    };
}