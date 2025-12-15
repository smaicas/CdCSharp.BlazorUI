namespace CdCSharp.BlazorUI.Core.Variants;

public interface IVariant
{
    string Name { get; }
}

public abstract class Variant : IVariant
{
    public string Name { get; }

    protected Variant(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

    public override bool Equals(object? obj) =>
        obj is Variant other &&
        GetType() == other.GetType() &&
        Name == other.Name;

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Name);

    public override string ToString() => Name;
}