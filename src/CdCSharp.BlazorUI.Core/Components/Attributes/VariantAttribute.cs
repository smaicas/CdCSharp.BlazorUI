using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Core.Components.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class VariantAttribute : Attribute
{
    public string VariantName { get; }
    protected VariantAttribute(string variantName) => VariantName = variantName;
}

[AttributeUsage(AttributeTargets.Method)]
public class VariantAttribute<TComponent, TVariant> : VariantAttribute
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    public VariantAttribute(string variantName) : base(variantName) { }
}
