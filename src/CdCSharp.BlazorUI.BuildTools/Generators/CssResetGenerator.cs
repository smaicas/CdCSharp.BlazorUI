using CdCSharp.BlazorUI.BuildTools.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

public class CssResetGenerator : IAssetGenerator
{
    private readonly BuildContext _context;

    public string Name => "CSS Reset";

    public CssResetGenerator(BuildContext context)
    {
        _context = context;
    }

    public async Task GenerateAsync()
    {
        string css = CssReset.GetCss();
        string outputPath = _context.GetFullPath("CssBundle/reset.css");
        await File.WriteAllTextAsync(outputPath, css);
    }
}

[ExcludeFromCodeCoverage]
public static class CssReset
{
    public static string GetCss() => """
        /* =========================================================
           CSS BASE / RESET
           Purpose: Normalize browser defaults while preserving
           semantic HTML behavior, accessibility and theming
           ========================================================= */

        /* ---------------------------------------------------------
           Design Tokens
           --------------------------------------------------------- */
        :root {
            /* Typography */
            --bui-font-base: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, Cantarell, "Helvetica Neue", Arial, sans-serif;
            --bui-font-heading: var(--bui-font-base);
            --bui-font-monospace: ui-monospace, "Cascadia Mono", "SF Mono", Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
            --bui-base-font-size: 16px;
            --bui-base-line-height: 1.5;
            --bui-heading-line-height: 1.2;
            /* Spacing */
            --bui-body-margin: 8px;
            --bui-space-1: 0.5em;
            --bui-space-2: 1em;
            /* Heading scale */
            --bui-h1-size: 2em;
            --bui-h2-size: 1.5em;
            --bui-h3-size: 1.17em;
            --bui-h4-size: 1em;
            --bui-h5-size: 0.83em;
            --bui-h6-size: 0.67em;
            /* UI states */
            --bui-focus-color: Highlight;
            --bui-focus-width: 2px;
            --bui-focus-offset: 2px;
            --bui-disabled-opacity: 0.5;
            --bui-placeholder-opacity: 0.6;
            /* Separators */
            --bui-hr-color: gray;
            --bui-hr-width: 1px;
            --bui-hr-style: inset;
            /* Tables */
            --bui-table-border-spacing: 2px;
            /* Media */
            --bui-media-max-width: 100%;
            /* Interaction */
            --bui-tap-highlight: transparent;
        }

        /* ---------------------------------------------------------
           Global Box Model & Neutral Reset
           --------------------------------------------------------- */
        :where(*, *::before, *::after) {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
            border: 0;
            background: transparent;
            color: inherit;
            font: inherit;
            text-decoration: none;
        }

        /* ---------------------------------------------------------
           Root & Body
           --------------------------------------------------------- */
        html {
            block-size: 100%;
            font-size: var(--bui-base-font-size);
        }

        body {
            margin: var(--bui-body-margin);
            font-family: var(--bui-font-base);
            font-size: 1rem;
            line-height: var(--bui-base-line-height);
        }

        /* ---------------------------------------------------------
           Structural Elements
           --------------------------------------------------------- */
        :where( header, main, footer, section, article, nav, aside, figure, figcaption, details, summary, dialog, form ) {
            display: block;
        }

        /* ---------------------------------------------------------
           Headings & Text Content
           --------------------------------------------------------- */
        :where(h1, h2, h3, h4, h5, h6) {
            font-family: var(--bui-font-heading);
            font-weight: bold;
            line-height: var(--bui-heading-line-height);
        }

        h1 {
            font-size: var(--bui-h1-size);
            margin-block: 0.67em;
        }

        h2 {
            font-size: var(--bui-h2-size);
            margin-block: 0.83em;
        }

        h3 {
            font-size: var(--bui-h3-size);
            margin-block: var(--bui-space-2);
        }

        h4 {
            font-size: var(--bui-h4-size);
            margin-block: 1.33em;
        }

        h5 {
            font-size: var(--bui-h5-size);
            margin-block: 1.67em;
        }

        h6 {
            font-size: var(--bui-h6-size);
            margin-block: 2.33em;
        }

        p,
        blockquote {
            margin-block: var(--bui-space-2);
        }

        blockquote {
            margin-inline: 40px;
        }

        code,
        pre,
        kbd,
        samp {
            font-family: var(--bui-font-monospace);
            font-size: 0.875em;
        }

        pre {
            white-space: pre;
        }

        /* ---------------------------------------------------------
           Lists
           --------------------------------------------------------- */
        ul,
        ol {
            margin-block: var(--bui-space-2);
            padding-inline-start: 40px;
        }

        ul {
            list-style: disc;
        }

        ol {
            list-style: decimal;
        }

        /* ---------------------------------------------------------
           Tables
           --------------------------------------------------------- */
        table {
            border-collapse: separate;
            border-spacing: var(--bui-table-border-spacing);
        }

        caption {
            text-align: center;
        }

        th {
            font-weight: bold;
            text-align: center;
        }

        /* ---------------------------------------------------------
           Media / Replaced Elements
           --------------------------------------------------------- */
        :where(img, svg, video, canvas, picture) {
            display: inline-block;
            max-width: var(--bui-media-max-width);
            height: auto;
        }

        /* ---------------------------------------------------------
           Horizontal Rule
           --------------------------------------------------------- */
        hr {
            margin-block: var(--bui-space-1);
            border: var(--bui-hr-width) var(--bui-hr-style) var(--bui-hr-color);
            overflow: hidden;
        }

        /* ---------------------------------------------------------
           Links
           --------------------------------------------------------- */
        a {
            cursor: pointer;
        }

        /* ---------------------------------------------------------
           Form Controls Reset
           --------------------------------------------------------- */
        :where( input[type="text"], input[type="search"], input[type="email"], input[type="password"], input[type="tel"], input[type="url"], input:not([type]), textarea, select, button, input[type="button"], input[type="submit"], input[type="reset"] ) {
            appearance: none;
            background: transparent;
            border: none;
            padding: 0;
            margin: 0;
            font: inherit;
            color: inherit;
            line-height: normal;
            box-sizing: border-box;
        }

        textarea {
            resize: vertical;
        }

        :where(button, input[type="button"], input[type="submit"], input[type="reset"]) {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
        }

        input[type="checkbox"],
        input[type="radio"] {
            appearance: none;
            cursor: pointer;
        }

        /* Disabled state */
        :where(button, input, select, textarea):disabled {
            cursor: default;
            opacity: var(--bui-disabled-opacity);
        }

        /* Autofill (WebKit) */
        :where(input, textarea, select):-webkit-autofill {
            box-shadow: 0 0 0 1000px transparent inset !important;
            -webkit-text-fill-color: inherit !important;
        }

        /* Placeholder */
        ::placeholder {
            color: inherit;
            opacity: var(--bui-placeholder-opacity);
        }

        /* ---------------------------------------------------------
           Focus & Accessibility
           --------------------------------------------------------- */
        :focus:not(:focus-visible) {
            outline: none;
        }

        :focus-visible {
            outline: var(--bui-focus-width) solid var(--bui-focus-color);
            outline-offset: var(--bui-focus-offset);
        }

        /* Touch interaction */
        :where(button, input, textarea, select, a) {
            -webkit-tap-highlight-color: var(--bui-tap-highlight);
        }

        /* ---------------------------------------------------------
           Utility Attributes
           --------------------------------------------------------- */
        [hidden] {
            display: none;
        }

        """;
}