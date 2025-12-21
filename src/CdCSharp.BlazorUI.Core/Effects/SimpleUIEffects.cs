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
    }

    public bool HasEffects => _effects.Any();

    public void AddEffect(EffectTrigger trigger, Action<EffectDefinition> configure)
    {
        EffectDefinition definition = new();
        configure(definition);
        _effects[trigger] = definition;
    }

    public string GenerateStyles(string componentId)
    {
        if (!_effects.Any()) return string.Empty;

        StringBuilder sb = new();

        // Base element styles
        List<string> baseProperties = [];
        foreach (EffectDefinition effect in _effects.Values)
        {
            if (effect.AnimationName == null)
            {
                // Collect all properties that need transition
                foreach (string prop in effect.Properties.Keys)
                {
                    if (!baseProperties.Contains(prop))
                    {
                        baseProperties.Add(prop);
                    }
                }
            }
        }

        if (baseProperties.Any())
        {
            sb.AppendLine($"[data-effect-id='{componentId}'] {{");
            sb.AppendLine($"  transition-property: {string.Join(", ", baseProperties)};");
            sb.AppendLine("}");
        }

        // Trigger-specific styles
        foreach ((EffectTrigger trigger, EffectDefinition? effect) in _effects)
        {
            string selector = $"[data-effect-id='{componentId}']{GetTriggerSelector(trigger)}";

            sb.AppendLine($"{selector} {{");

            // Add properties
            foreach (KeyValuePair<string, string> prop in effect.Properties)
            {
                sb.AppendLine($"  {prop.Key}: {prop.Value};");
            }

            // Add transition/animation
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

        return sb.ToString();
    }

    private string GetTriggerSelector(EffectTrigger trigger) => trigger switch
    {
        EffectTrigger.Hover => ":hover",
        EffectTrigger.HoverEnd => ":not(:hover)",
        EffectTrigger.Focus => ":focus",
        EffectTrigger.Blur => ":not(:focus)",
        EffectTrigger.Active => ":active",
        EffectTrigger.Click => ":active", // CSS limitation
        EffectTrigger.Disabled => ":disabled",
        EffectTrigger.Enabled => ":not(:disabled)",
        _ => ""
    };
}