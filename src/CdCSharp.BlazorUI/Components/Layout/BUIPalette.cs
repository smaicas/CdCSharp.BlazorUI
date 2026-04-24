namespace CdCSharp.BlazorUI.Components.Layout;

public sealed class BUIPalette
{
    public CssColor Background { get; }
    public CssColor BackgroundContrast { get; }
    public CssColor Error { get; }
    public CssColor ErrorContrast { get; }
    public CssColor Info { get; }
    public CssColor InfoContrast { get; }
    public CssColor Primary { get; }
    public CssColor PrimaryContrast { get; }
    public CssColor Secondary { get; }
    public CssColor SecondaryContrast { get; }
    public CssColor Shadow { get; }
    public CssColor Success { get; }
    public CssColor SuccessContrast { get; }
    public CssColor Surface { get; }
    public CssColor SurfaceContrast { get; }
    public CssColor Warning { get; }
    public CssColor WarningContrast { get; }

    public BUIPalette(IReadOnlyDictionary<string, string> palette)
    {
        CssColor C(string key)
        {
            return new(palette[key]);
        }

        Background = C("--palette-background");
        BackgroundContrast = C("--palette-backgroundcontrast");
        Error = C("--palette-error");
        ErrorContrast = C("--palette-errorcontrast");
        Info = C("--palette-info");
        InfoContrast = C("--palette-infocontrast");
        Primary = C("--palette-primary");
        PrimaryContrast = C("--palette-primarycontrast");
        Secondary = C("--palette-secondary");
        SecondaryContrast = C("--palette-secondarycontrast");
        Shadow = C("--palette-shadow");
        Success = C("--palette-success");
        SuccessContrast = C("--palette-successcontrast");
        Surface = C("--palette-surface");
        SurfaceContrast = C("--palette-surfacecontrast");
        Warning = C("--palette-warning");
        WarningContrast = C("--palette-warningcontrast");
    }
}