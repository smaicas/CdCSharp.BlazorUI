namespace CdCSharp.BlazorUI.BuildTools.Generators;

public interface IAssetGenerator
{
    string Name { get; }
    Task GenerateAsync();
}
