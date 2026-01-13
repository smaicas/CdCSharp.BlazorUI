using System.Globalization;

namespace CdCSharp.BlazorUI.Localization.Wasm;

public class LocalizationSettings
{
    public string DefaultCulture { get; set; } = "en-US";

    public string ResourcesPath { get; set; } = "Resources";

    public List<CultureInfo> SupportedCultures { get; set; } =
        [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES")
    ];
}