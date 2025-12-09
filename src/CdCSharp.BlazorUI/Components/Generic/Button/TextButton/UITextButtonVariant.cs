using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Generic.Button.TextButton;

public class UITextButtonVariant : Variant
{
    private UITextButtonVariant(string name) : base(name) { }

    // Variantes predefinidas
    public static readonly UITextButtonVariant Default = new(nameof(Default));

    // Custom disponible para este componente
    public static UITextButtonVariant Custom(string name) => new(name);
}
