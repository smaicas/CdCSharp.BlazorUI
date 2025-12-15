using CdCSharp.BlazorUI.Core.Variants;

namespace CdCSharp.BlazorUI.Components.Generic.Button;

public class UIButtonVariant : Variant
{
    public UIButtonVariant(string name) : base(name)
    {
    }
    public static readonly UIButtonVariant Default = new("Default");
    public static UIButtonVariant Custom(string name) => new(name);
}
