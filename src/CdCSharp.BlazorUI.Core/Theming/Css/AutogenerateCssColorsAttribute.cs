namespace CdCSharp.BlazorUI.Core.Theming.Css;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AutogenerateCssColorsAttribute : Attribute
{
    public int VariantLevels { get; }

    public AutogenerateCssColorsAttribute(int variantLevels = 5)
    {
        if (variantLevels <= 0)
        {
            throw new ArgumentException("Variant levels must be greater than 0", nameof(variantLevels));
        }

        VariantLevels = variantLevels;
    }
}