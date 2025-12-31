using CdCSharp.BlazorUI.Abstractions.Behaviors.Design;

namespace CdCSharp.BlazorUI.Components;

public static class BUIBorderPresets
{
    // Bordes básicos sin radius
    public static readonly BorderStyle Default = new("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2);
    public static readonly BorderStyle Subtle = new("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten3);
    public static readonly BorderStyle Strong = new("2px", BorderStyleType.Solid, BUIColor.Gray.Default);

    // Bordes temáticos
    public static readonly BorderStyle Primary = new("2px", BorderStyleType.Solid, BUIColor.Palette.Primary);
    public static readonly BorderStyle Secondary = new("2px", BorderStyleType.Solid, BUIColor.Palette.Secondary);
    public static readonly BorderStyle Error = new("2px", BorderStyleType.Solid, BUIColor.Palette.Error);
    public static readonly BorderStyle Warning = new("2px", BorderStyleType.Solid, BUIColor.Palette.Warning);
    public static readonly BorderStyle Success = new("2px", BorderStyleType.Solid, BUIColor.Palette.Success);
    public static readonly BorderStyle Info = new("2px", BorderStyleType.Solid, BUIColor.Palette.Info);

    // Bordes con estilos especiales
    public static readonly BorderStyle Dashed = new("1px", BorderStyleType.Dashed, BUIColor.Gray.Default);
    public static readonly BorderStyle Dotted = new("1px", BorderStyleType.Dotted, BUIColor.Gray.Default);
    public static readonly BorderStyle Double = new("3px", BorderStyleType.Double, BUIColor.Gray.Default);

    // Bordes con radius predefinidos
    public static readonly BorderStyle Rounded = new("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2, 4);
    public static readonly BorderStyle RoundedLarge = new("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2, 8);
    public static readonly BorderStyle Pill = new("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2, 9999);
}