using CdCSharp.BlazorUI.BuildTools.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

public class CssInitializeThemesGenerator : IAssetGenerator
{
    private readonly BuildContext _context;

    public string Name => "Initialize Themes CSS";

    public CssInitializeThemesGenerator(BuildContext context)
    {
        _context = context;
    }

    public async Task GenerateAsync()
    {
        string css = CssInitializeTheme.GetCss();
        string outputPath = _context.GetFullPath("CssBundle/initialize-themes.css");
        await File.WriteAllTextAsync(outputPath, css);
    }
}

[ExcludeFromCodeCoverage]
public static class CssInitializeTheme
{
    public static string GetCss() => """
        body {
          background-color: var(--palette-background);
          color: var(--palette-backgroundcontrast);
        }

        .ui-color-primary {
          color: var(--palette-primary);
        }

        .ui-bg-primary {
          background-color: var(--palette-primary);
        }

        .ui-color-secondary {
          color: var(--palette-secondary);
        }

        .ui-bg-secondary {
          background-color: var(--palette-secondary);
        }

        .ui-color-success {
          color: var(--palette-success);
        }

        .ui-bg-success {
          background-color: var(--palette-success);
        }

        .ui-color-warning {
          color: var(--palette-warning);
        }

        .ui-bg-warning {
          background-color: var(--palette-warning);
        }

        .ui-color-error {
          color: var(--palette-error);
        }

        .ui-bg-error {
          background-color: var(--palette-error);
        }

        .ui-color-info {
          color: var(--palette-info);
        }

        .ui-bg-info {
          background-color: var(--palette-info);
        }

        .ui-primary {
          color: var(--palette-primarycontrast);
          background-color: var(--palette-primary);
        }

        .ui-secondary {
          color: var(--palette-secondarycontrast);
          background-color: var(--palette-secondary);
        }
        """;
}