namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public abstract class CssEffect
{
    public string Name { get; protected set; }
    public abstract string ToCss();
    public abstract string ToInlineCss();
}

public class CssEffectCollection : List<CssEffect>
{
    public string ToCss() => string.Join(" ", this.Select(e => e.ToCss()));
    public string ToInlineCss() => string.Join("; ", this.Select(e => e.ToInlineCss()));
}
