namespace CdCSharp.BlazorUI.Core.Effects.Builders;

public class SimpleEffectBuilder
{
    private readonly SimpleUIEffects _effects = new();

    public SimpleEffectBuilder OnHover(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.Hover, configure);
        return this;
    }

    public SimpleEffectBuilder OnFocus(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.Focus, configure);
        return this;
    }

    public SimpleEffectBuilder OnActive(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.Active, configure);
        return this;
    }

    public SimpleEffectBuilder OnClick(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.Click, configure);
        return this;
    }

    public SimpleEffectBuilder OnDoubleClick(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.DoubleClick, configure);
        return this;
    }

    public SimpleEffectBuilder OnDisabled(Action<SimpleUIEffects.EffectDefinition> configure)
    {
        _effects.AddEffect(EffectTrigger.Disabled, configure);
        return this;
    }

    public SimpleUIEffects Build() => _effects;

    // Static factory method
    public static SimpleEffectBuilder Create() => new();
}

public enum EffectTrigger
{
    Hover,
    HoverEnd,
    Click,
    DoubleClick,
    MouseDown,
    MouseUp,
    Focus,
    Blur,
    Active,
    Disabled,
    Enabled,
    Appear,
    Disappear,
    TouchStart,
    TouchEnd,
    Custom
}