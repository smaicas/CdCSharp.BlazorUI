using CdCSharp.BlazorUI.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Forms.Text;

public class InputTextVariant : Variant
{
    public static readonly InputTextVariant Outlined = new("Outlined");
    public static readonly InputTextVariant Filled = new("Filled");
    public static readonly InputTextVariant Standard = new("Standard");

    public InputTextVariant(string name) : base(name) { }

    public static InputTextVariant Custom(string name) => new(name);
}
