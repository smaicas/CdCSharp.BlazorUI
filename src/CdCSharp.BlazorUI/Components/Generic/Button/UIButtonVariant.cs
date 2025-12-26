using CdCSharp.BlazorUI.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Generic.Button;

public class UIButtonVariant : Variant
{
    public static readonly UIButtonVariant Default = new("Default");

    public UIButtonVariant(string name) : base(name)
    {
    }

    public static UIButtonVariant Custom(string name) => new(name);
}