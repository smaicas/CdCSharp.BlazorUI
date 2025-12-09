using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Generic.Button;

public sealed class UIButtonVariant : Variant
{
    private UIButtonVariant(string name) : base(name) { }

    // Variantes predefinidas
    public static readonly UIButtonVariant Primary = new("Primary");
    public static readonly UIButtonVariant Secondary = new("Secondary");
    public static readonly UIButtonVariant Success = new("Success");
    public static readonly UIButtonVariant Danger = new("Danger");

    // Custom disponible para este componente
    public static UIButtonVariant Custom(string name) => new(name);
}
