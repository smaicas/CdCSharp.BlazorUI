using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

namespace CdCSharp.BlazorUI.Components;

public static class BUIBorderPresets
{
    // =====================================
    // Básicos (sin radius)
    // =====================================

    public static BorderStyle Default =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2);

    public static BorderStyle Subtle =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten3);

    public static BorderStyle Strong =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Gray.Default);

    // =====================================
    // Temáticos
    // =====================================

    public static BorderStyle Primary =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Primary);

    public static BorderStyle Secondary =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Secondary);

    public static BorderStyle Error =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Error);

    public static BorderStyle Warning =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Warning);

    public static BorderStyle Success =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Success);

    public static BorderStyle Info =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Palette.Info);

    // =====================================
    // Estilos especiales
    // =====================================

    public static BorderStyle Dashed =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Dashed, BUIColor.Gray.Default);

    public static BorderStyle Dotted =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Dotted, BUIColor.Gray.Default);

    public static BorderStyle Double =>
        BorderStyle.Create()
            .All("3px", BorderStyleType.Double, BUIColor.Gray.Default);

    // =====================================
    // Radius predefinido
    // =====================================

    public static BorderStyle Rounded =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(4);

    public static BorderStyle RoundedLarge =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(8);

    public static BorderStyle Pill =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(9999);

    // =====================================
    // Utilitarios
    // =====================================

    public static BorderStyle None =>
        BorderStyle.Create().None();
}
