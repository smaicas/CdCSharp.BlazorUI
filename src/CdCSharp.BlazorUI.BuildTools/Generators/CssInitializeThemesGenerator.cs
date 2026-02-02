using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class CssInitializeThemesGenerator : IAssetGenerator
{
    public string FileName => "_initialize-themes.css";
    public string Name => "Initialize Themes CSS";

    public async Task<string> GetContent() => $$"""
        body {
          font-family: var({{FeatureDefinitions.Typography.FontFamily}});
          font-size: var({{FeatureDefinitions.Typography.FontSizeBase}});
          line-height: var({{FeatureDefinitions.Typography.LineHeight}});
          background-color: var(--palette-background);
          color: var(--palette-backgroundcontrast);
          background-color: var(--palette-background);
          color: var(--palette-backgroundcontrast);
        }

        .bui-color-primary {
          color: var(--palette-primary);
        }

        .bui-bg-primary {
          background-color: var(--palette-primary);
        }

        .bui-color-secondary {
          color: var(--palette-secondary);
        }

        .bui-bg-secondary {
          background-color: var(--palette-secondary);
        }

        .bui-color-success {
          color: var(--palette-success);
        }

        .bui-bg-success {
          background-color: var(--palette-success);
        }

        .bui-color-warning {
          color: var(--palette-warning);
        }

        .bui-bg-warning {
          background-color: var(--palette-warning);
        }

        .bui-color-error {
          color: var(--palette-error);
        }

        .bui-bg-error {
          background-color: var(--palette-error);
        }

        .bui-color-info {
          color: var(--palette-info);
        }

        .bui-bg-info {
          background-color: var(--palette-info);
        }

        .bui-primary {
          color: var(--palette-primarycontrast);
          background-color: var(--palette-primary);
        }

        .bui-secondary {
          color: var(--palette-secondarycontrast);
          background-color: var(--palette-secondary);
        }
        """;
}