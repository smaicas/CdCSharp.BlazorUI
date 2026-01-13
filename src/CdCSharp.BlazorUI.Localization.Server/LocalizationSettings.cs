using System.Globalization;

namespace CdCSharp.BlazorUI.Localization.Server;

public class LocalizationSettings
{
    /// <summary>
    /// Cookie name for Server implementation
    /// </summary>
    public string CultureCookieName { get; set; } = ".BlazorUI.Culture";

    public string DefaultCulture { get; set; } = "en-US";

    public string ResourcesPath { get; set; } = "Resources";

    public List<CultureInfo> SupportedCultures { get; set; } =
        [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES")
    ];
}