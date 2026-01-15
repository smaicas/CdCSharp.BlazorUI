namespace CdCSharp.BlazorUI.Components;

public interface IVariantComponent
{
    Variant CurrentVariant { get; }
    Type VariantType { get; }
}

public interface IVariantComponent<TVariant> : IVariantComponent
    where TVariant : Variant
{
    new TVariant CurrentVariant { get; }
    TVariant DefaultVariant { get; }
    TVariant? Variant { get; set; }
}