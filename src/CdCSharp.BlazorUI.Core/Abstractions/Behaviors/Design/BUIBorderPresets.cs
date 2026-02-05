using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Components;

public static class BUIBorderPresets
{
    // ===================================== Básicos (sin radius) =====================================

    public static BorderStyle Dashed =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Dashed, BUIColor.Gray.Default);

    public static BorderStyle Default =>
            BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2);

    // ===================================== Estilos especiales =====================================
    public static BorderStyle Dotted =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Dotted, BUIColor.Gray.Default);

    public static BorderStyle Double =>
        BorderStyle.Create()
            .All("3px", BorderStyleType.Double, BUIColor.Gray.Default);

    public static BorderStyle Error =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Error);

    public static BorderStyle Info =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Info);

    public static BorderStyle None =>
        BorderStyle.Create().None();

    public static BorderStyle Pill =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(9999);

    public static BorderStyle Primary =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Primary);

    public static BorderStyle Rounded =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(4);

    // ===================================== Radius predefinido =====================================
    public static BorderStyle RoundedLarge =>
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten2)
            .Radius(8);

    // ===================================== Temáticos =====================================
    public static BorderStyle Secondary =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Secondary);

    public static BorderStyle Strong =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, BUIColor.Gray.Default);

    public static BorderStyle Subtle => 
        BorderStyle.Create()
            .All("1px", BorderStyleType.Solid, BUIColor.Gray.Lighten3);

    public static BorderStyle Success =>
        BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Success);

    public static BorderStyle Warning =>
            BorderStyle.Create()
            .All("2px", BorderStyleType.Solid, PaletteColor.Warning);

    // ===================================== Utilitarios =====================================
}