namespace CdCSharp.BuildTools;

public interface IAssetGenerator
{
    string FileName { get; }
    string Name { get; }

    Task<string> GetContent();
}