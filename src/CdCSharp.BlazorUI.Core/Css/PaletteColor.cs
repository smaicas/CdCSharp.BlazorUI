using System;
using System.Collections.Generic;
using System.Text;

namespace CdCSharp.BlazorUI.Components;

public sealed class PaletteColor
{
    private readonly string _variable;

    private PaletteColor(string variable) => _variable = variable;

    public static PaletteColor Background => new("--palette-background");
    public static PaletteColor BackgroundContrast => new("--palette-backgroundcontrast");
    public static PaletteColor Error => new("--palette-error");
    public static PaletteColor ErrorContrast => new("--palette-errorcontrast");
    public static PaletteColor Info => new("--palette-info");
    public static PaletteColor InfoContrast => new("--palette-infocontrast");
    public static PaletteColor Primary => new("--palette-primary");
    public static PaletteColor PrimaryContrast => new("--palette-primarycontrast");
    public static PaletteColor Secondary => new("--palette-secondary");
    public static PaletteColor SecondaryContrast => new("--palette-secondarycontrast");
    public static PaletteColor Success => new("--palette-success");
    public static PaletteColor SuccessContrast => new("--palette-successcontrast");
    public static PaletteColor Surface => new("--palette-surface");
    public static PaletteColor SurfaceContrast => new("--palette-surfacecontrast");
    public static PaletteColor Warning => new("--palette-warning");
    public static PaletteColor WarningContrast => new("--palette-warningcontrast");
    public static PaletteColor Black => new("--palette-black");
    public static PaletteColor Border => new("--palette-border");
    public static PaletteColor Highlight => new("--palette-highlight");
    public static PaletteColor Shadow => new("--palette-shadow");
    public static PaletteColor White => new("--palette-white");

    public static implicit operator string(PaletteColor p) => $"var({p._variable})";

    public override string ToString() => $"var({_variable})";
}
