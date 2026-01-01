using System.Globalization;

namespace CdCSharp.BlazorUI.Localization.Abstractions;

public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "en-US";

    public List<CultureInfo> SupportedCultures { get; set; } =
    [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES")
    ];

    public string ResourcesPath { get; set; } = "Resources";

    /// <summary>
    /// Cookie name for Server implementation
    /// </summary>
    public string CultureCookieName { get; set; } = ".AspNetCore.Culture";
}
