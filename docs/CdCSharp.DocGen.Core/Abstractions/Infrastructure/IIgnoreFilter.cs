namespace CdCSharp.DocGen.Core.Abstractions.Infrastructure;

public interface IIgnoreFilter
{
    string Source { get; }
    bool IsIgnored(string path);
    Task InitializeAsync(string projectPath);
}