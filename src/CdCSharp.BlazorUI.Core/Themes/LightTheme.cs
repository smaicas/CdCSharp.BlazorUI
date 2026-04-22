using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;

namespace CdCSharp.BlazorUI.Core.Themes;

/// <summary>
/// Light theme — Porcelain / Ink Neutral, editorial y muy profesional, inspirado en diseño impreso
/// de alta gama.
/// </summary>
public sealed class LightTheme : BUIThemePaletteBase
{
    public LightTheme()
    {
        Id = "light";
        Name = "Light";

        Background = new CssColor("#F2F2F0");
        // Gris porcelana cálido: base neutra, elegante y menos fatigante que el blanco puro.

        BackgroundContrast = new CssColor("#1F2328");
        // Tinta casi negra: máxima legibilidad sin usar negro absoluto.

        Surface = new CssColor("#FFFFFFF0".Substring(0, 7));
        // Blanco suavizado: crea una capa claramente elevada respecto al fondo.

        SurfaceContrast = new CssColor("#1F2328");
        // Consistencia tipográfica y contraste AAA.

        Error = new CssColor("#B94A48");
        // Rojo óxido apagado: serio y profesional, evita el dramatismo.

        ErrorContrast = new CssColor("#FFFFFF");
        // Alta legibilidad en mensajes críticos.

        Success = new CssColor("#4A7C59");
        // Verde bosque desaturado: transmite estabilidad y confianza.

        SuccessContrast = new CssColor("#FFFFFF");
        // Contraste limpio y armónico.

        Warning = new CssColor("#B5893D");
        // Mostaza profunda: advertencia elegante, nada estridente.

        WarningContrast = new CssColor("#1F2328");
        // Oscuro para asegurar lectura clara.

        Info = new CssColor("#5A6F8A");
        // Azul pizarra neutro: informativo y sobrio, sin clichés SaaS.

        InfoContrast = new CssColor("#FFFFFF");
        // Claridad visual en estados informativos.

        Primary = new CssColor("#2F3E46");
        // Gris azulado tinta: identidad principal seria, corporativa y moderna.

        PrimaryContrast = new CssColor("#FFFFFF");
        // Alto contraste para CTAs.

        Secondary = new CssColor("#6D6875");
        // Malva grisáceo: sofisticado y editorial, acompaña sin competir.

        SecondaryContrast = new CssColor("#FFFFFF");
        // Legibilidad consistente.

        Shadow = new CssColor("#000000");
        // Sombra marcada y limpia: separación clara entre planos.

        Border = new CssColor("#111111");

        Highlight = new CssColor("#AA2222");
    }
}