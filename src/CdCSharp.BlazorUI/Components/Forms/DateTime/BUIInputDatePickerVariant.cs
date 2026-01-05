using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputDatePickerVariant : Variant
{
    public static readonly BUIInputDatePickerVariant Outlined = new("Outlined");
    public static readonly BUIInputDatePickerVariant Filled = new("Filled");
    public static readonly BUIInputDatePickerVariant Standard = new("Standard");

    public BUIInputDatePickerVariant(string name) : base(name) { }

    public static BUIInputDatePickerVariant Custom(string name) => new(name);
}