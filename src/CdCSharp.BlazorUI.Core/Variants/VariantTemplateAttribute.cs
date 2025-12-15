namespace CdCSharp.BlazorUI.Core.Variants;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
public sealed class VariantTemplateAttribute : Attribute
{
    public Type ComponentType { get; }
    public Type VariantType { get; }
    public string VariantName { get; }

    public VariantTemplateAttribute(Type componentType, Type variantType, string variantName)
    {
        ComponentType = componentType;
        VariantType = variantType;
        VariantName = variantName;
    }
}
