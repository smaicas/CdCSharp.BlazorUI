using CdCSharp.BlazorUI.Css;

namespace CdCSharp.BlazorUI.Components.Features.Common;

public static class BorderPresets
{
    // Bordes básicos sin radius
    public static readonly BorderStyle Default = new("1px", BorderStyleType.Solid, UIColor.Gray.Lighten2);
    public static readonly BorderStyle Subtle = new("1px", BorderStyleType.Solid, UIColor.Gray.Lighten3);
    public static readonly BorderStyle Strong = new("2px", BorderStyleType.Solid, UIColor.Gray.Default);

    // Bordes temáticos
    public static readonly BorderStyle Primary = new("2px", BorderStyleType.Solid, UIColor.Palette.Primary);
    public static readonly BorderStyle Secondary = new("2px", BorderStyleType.Solid, UIColor.Palette.Secondary);
    public static readonly BorderStyle Error = new("2px", BorderStyleType.Solid, UIColor.Palette.Error);
    public static readonly BorderStyle Warning = new("2px", BorderStyleType.Solid, UIColor.Palette.Warning);
    public static readonly BorderStyle Success = new("2px", BorderStyleType.Solid, UIColor.Palette.Success);
    public static readonly BorderStyle Info = new("2px", BorderStyleType.Solid, UIColor.Palette.Info);

    // Bordes con estilos especiales
    public static readonly BorderStyle Dashed = new("1px", BorderStyleType.Dashed, UIColor.Gray.Default);
    public static readonly BorderStyle Dotted = new("1px", BorderStyleType.Dotted, UIColor.Gray.Default);
    public static readonly BorderStyle Double = new("3px", BorderStyleType.Double, UIColor.Gray.Default);

    // Bordes con radius predefinidos
    public static readonly BorderStyle Rounded = new("1px", BorderStyleType.Solid, UIColor.Gray.Lighten2, 4);
    public static readonly BorderStyle RoundedLarge = new("1px", BorderStyleType.Solid, UIColor.Gray.Lighten2, 8);
    public static readonly BorderStyle Pill = new("1px", BorderStyleType.Solid, UIColor.Gray.Lighten2, 9999);
}