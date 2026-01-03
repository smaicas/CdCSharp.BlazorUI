namespace CdCSharp.BuildTools;

public interface IAssetGenerator
{
    string Name { get; }
    string FileName { get; }
    Task<string> GetContent();
}
