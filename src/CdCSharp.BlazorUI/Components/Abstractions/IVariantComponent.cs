namespace CdCSharp.BlazorUI.Components.Abstractions;

public interface IVariantComponent
{
    Variant CurrentVariant { get; }
    Type VariantType { get; }
}

public interface IVariantComponent<TVariant> : IVariantComponent
    where TVariant : Variant
{
    TVariant? Variant { get; set; }
    TVariant DefaultVariant { get; }
    new TVariant CurrentVariant { get; }
}
