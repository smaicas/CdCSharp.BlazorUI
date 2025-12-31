using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Components;

[AutogenerateCssColors(3)]
public static partial class BUIColor
{
    public static class Palette
    {
        // Surface
        public static CssColor Background =>
            new("var(--palette-background)", true);

        public static CssColor BackgroundContrast =>
            new("var(--palette-backgroundcontrast)", true);

        public static CssColor Surface =>
            new("var(--palette-surface)", true);

        public static CssColor SurfaceContrast =>
            new("var(--palette-surfacecontrast)", true);

        // Main
        public static CssColor Primary =>
            new("var(--palette-primary)", true);

        public static CssColor PrimaryContrast =>
            new("var(--palette-primarycontrast)", true);

        public static CssColor Secondary =>
            new("var(--palette-secondary)", true);

        public static CssColor SecondaryContrast =>
            new("var(--palette-secondarycontrast)", true);

        // Status
        public static CssColor Success =>
            new("var(--palette-success)", true);

        public static CssColor SuccessContrast =>
            new("var(--palette-successcontrast)", true);

        public static CssColor Warning =>
            new("var(--palette-warning)", true);

        public static CssColor WarningContrast =>
            new("var(--palette-warningcontrast)", true);

        public static CssColor Error =>
            new("var(--palette-error)", true);

        public static CssColor ErrorContrast =>
            new("var(--palette-errorcontrast)", true);

        public static CssColor Info =>
            new("var(--palette-info)", true);

        public static CssColor InfoContrast =>
            new("var(--palette-infocontrast)", true);
    }
}