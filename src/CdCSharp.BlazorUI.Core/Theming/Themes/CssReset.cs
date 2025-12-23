using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Core.Theming.Themes;

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
            --blazorui-font-base: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, Cantarell, "Helvetica Neue", Arial, sans-serif;
            --blazorui-font-heading: var(--blazorui-font-base);
            --blazorui-font-monospace: ui-monospace, "Cascadia Mono", "SF Mono", Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
            --blazorui-base-font-size: 16px;
            --blazorui-base-line-height: 1.5;
            --blazorui-heading-line-height: 1.2;
            /* Spacing */
            --blazorui-body-margin: 8px;
            --blazorui-space-1: 0.5em;
            --blazorui-space-2: 1em;
            /* Heading scale */
            --blazorui-h1-size: 2em;
            --blazorui-h2-size: 1.5em;
            --blazorui-h3-size: 1.17em;
            --blazorui-h4-size: 1em;
            --blazorui-h5-size: 0.83em;
            --blazorui-h6-size: 0.67em;
            /* UI states */
            --blazorui-focus-color: Highlight;
            --blazorui-focus-width: 2px;
            --blazorui-focus-offset: 2px;
            --blazorui-disabled-opacity: 0.5;
            --blazorui-placeholder-opacity: 0.6;
            /* Separators */
            --blazorui-hr-color: gray;
            --blazorui-hr-width: 1px;
            --blazorui-hr-style: inset;
            /* Tables */
            --blazorui-table-border-spacing: 2px;
            /* Media */
            --blazorui-media-max-width: 100%;
            /* Interaction */
            --blazorui-tap-highlight: transparent;
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
            font-size: var(--blazorui-base-font-size);
        }

        body {
            margin: var(--blazorui-body-margin);
            font-family: var(--blazorui-font-base);
            font-size: 1rem;
            line-height: var(--blazorui-base-line-height);
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
            font-family: var(--blazorui-font-heading);
            font-weight: bold;
            line-height: var(--blazorui-heading-line-height);
        }

        h1 {
            font-size: var(--blazorui-h1-size);
            margin-block: 0.67em;
        }

        h2 {
            font-size: var(--blazorui-h2-size);
            margin-block: 0.83em;
        }

        h3 {
            font-size: var(--blazorui-h3-size);
            margin-block: var(--blazorui-space-2);
        }

        h4 {
            font-size: var(--blazorui-h4-size);
            margin-block: 1.33em;
        }

        h5 {
            font-size: var(--blazorui-h5-size);
            margin-block: 1.67em;
        }

        h6 {
            font-size: var(--blazorui-h6-size);
            margin-block: 2.33em;
        }

        p,
        blockquote {
            margin-block: var(--blazorui-space-2);
        }

        blockquote {
            margin-inline: 40px;
        }

        code,
        pre,
        kbd,
        samp {
            font-family: var(--blazorui-font-monospace);
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
            margin-block: var(--blazorui-space-2);
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
            border-spacing: var(--blazorui-table-border-spacing);
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
            max-width: var(--blazorui-media-max-width);
            height: auto;
        }

        /* ---------------------------------------------------------
           Horizontal Rule
           --------------------------------------------------------- */
        hr {
            margin-block: var(--blazorui-space-1);
            border: var(--blazorui-hr-width) var(--blazorui-hr-style) var(--blazorui-hr-color);
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
            opacity: var(--blazorui-disabled-opacity);
        }

        /* Autofill (WebKit) */
        :where(input, textarea, select):-webkit-autofill {
            box-shadow: 0 0 0 1000px transparent inset !important;
            -webkit-text-fill-color: inherit !important;
        }

        /* Placeholder */
        ::placeholder {
            color: inherit;
            opacity: var(--blazorui-placeholder-opacity);
        }

        /* ---------------------------------------------------------
           Focus & Accessibility
           --------------------------------------------------------- */
        :focus:not(:focus-visible) {
            outline: none;
        }

        :focus-visible {
            outline: var(--blazorui-focus-width) solid var(--blazorui-focus-color);
            outline-offset: var(--blazorui-focus-offset);
        }

        /* Touch interaction */
        :where(button, input, textarea, select, a) {
            -webkit-tap-highlight-color: var(--blazorui-tap-highlight);
        }

        /* ---------------------------------------------------------
           Utility Attributes
           --------------------------------------------------------- */
        [hidden] {
            display: none;
        }

        """;
}