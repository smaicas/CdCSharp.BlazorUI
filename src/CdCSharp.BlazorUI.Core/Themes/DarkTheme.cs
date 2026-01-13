using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;

namespace CdCSharp.BlazorUI.Core.Themes;

/// <summary>
/// Dark theme — Carbon / Mineral
/// Oscuro técnico y moderno, con acentos minerales y sensación de precisión.
/// </summary>
public class DarkTheme : BUIThemePaletteBase
{
    public DarkTheme()
    {
        Id = "dark";
        Name = "Dark";

        Background = new CssColor("#121417");
        // Carbón neutro: oscuro profundo, sin matices azulados o marrones.

        BackgroundContrast = new CssColor("#ECEFF1");
        // Gris muy claro: lectura cómoda y limpia.

        Surface = new CssColor("#1E2126");
        // Gris grafito elevado: diferencia clara respecto al fondo base.

        SurfaceContrast = new CssColor("#ECEFF1");
        // Contraste estable para texto prolongado.

        Error = new CssColor("#CF6679");
        // Rojo rosado técnico: visible y moderno, evita el rojo clásico.

        ErrorContrast = new CssColor("#121417");
        // Mantiene equilibrio visual.

        Success = new CssColor("#4DD4A3");
        // Verde mineral frío: sensación de sistema confiable y tecnológico.

        SuccessContrast = new CssColor("#121417");
        // Contraste suficiente sin efecto glow.

        Warning = new CssColor("#E6B566");
        // Arena metálica: advertencia sofisticada, legible en dark mode.

        WarningContrast = new CssColor("#121417");
        // Oscuro para claridad inmediata.

        Info = new CssColor("#7AA2F7");
        // Azul lavanda técnico: informativo y moderno, no corporativo.

        InfoContrast = new CssColor("#121417");
        // Buen contraste sin romper la estética.

        Primary = new CssColor("#8AB4F8");
        // Azul mineral luminoso: foco principal claro y contemporáneo.

        PrimaryContrast = new CssColor("#121417");
        // Accesibilidad y sobriedad.

        Secondary = new CssColor("#C792EA");
        // Violeta tecnológico: creativo y distintivo, muy moderno.

        SecondaryContrast = new CssColor("#121417");
        // Lectura clara en elementos secundarios.

        Shadow = new CssColor("#777777");
        // Sombra profunda y densa: sensación real de elevación y capas.

        Border = new CssColor("#EEEEEE");

        Highlight = new CssColor("#AA2222");
    }
}
