namespace CdCSharp.BuildTools.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AssetGeneratorAttribute : Attribute
{
    public int Order { get; init; } = 0;
}