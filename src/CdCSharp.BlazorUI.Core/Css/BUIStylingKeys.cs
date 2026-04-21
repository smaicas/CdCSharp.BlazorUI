namespace CdCSharp.BlazorUI.Core.Css;

/// <summary>
/// Public, stable subset of the BlazorUI CSS/DOM contract that consumers can reference
/// when writing custom CSS or tests against BUI components.
/// </summary>
/// <remarks>
/// The internal contract lives in <c>CdCSharp.BlazorUI.Components.FeatureDefinitions</c>
/// and may change between minor versions; the keys exposed here are the subset that
/// the library commits to keep stable across 1.x.
/// </remarks>
public static class BUIStylingKeys
{
    /// <summary>
    /// <c>data-bui-component</c> — identifies the component type on the root
    /// <c>&lt;bui-component&gt;</c> element (e.g. <c>button</c>, <c>input-text</c>).
    /// </summary>
    public const string Component = "data-bui-component";

    /// <summary>
    /// <c>data-bui-size</c> — small | medium | large. Drives
    /// <see cref="InlineSizeMultiplier"/>.
    /// </summary>
    public const string Size = "data-bui-size";

    /// <summary>
    /// <c>data-bui-density</c> — compact | standard | comfortable. Drives
    /// <see cref="InlineDensityMultiplier"/>.
    /// </summary>
    public const string Density = "data-bui-density";

    /// <summary>
    /// <c>data-bui-variant</c> — current variant name (e.g. <c>outlined</c>,
    /// <c>filled</c>, a custom variant registered via <c>AddBlazorUIVariants</c>).
    /// </summary>
    public const string Variant = "data-bui-variant";

    /// <summary>
    /// <c>--bui-inline-color</c> — inline foreground-color override emitted from
    /// <c>IHasColor</c>.
    /// </summary>
    public const string InlineColor = "--bui-inline-color";

    /// <summary>
    /// <c>--bui-inline-background</c> — inline background-color override emitted
    /// from <c>IHasBackgroundColor</c>.
    /// </summary>
    public const string InlineBackground = "--bui-inline-background";

    /// <summary>
    /// <c>--bui-size-multiplier</c> — scalar multiplier applied to component
    /// dimensions based on <see cref="Size"/>.
    /// </summary>
    public const string InlineSizeMultiplier = "--bui-size-multiplier";

    /// <summary>
    /// <c>--bui-density-multiplier</c> — scalar multiplier applied to inter-element
    /// spacing based on <see cref="Density"/>.
    /// </summary>
    public const string InlineDensityMultiplier = "--bui-density-multiplier";
}
