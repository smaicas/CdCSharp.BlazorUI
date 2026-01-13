namespace CdCSharp.BuildTools.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BuildTemplateAttribute : Attribute
{
    public BuildTemplateAttribute(string relativePath)
    {
        RelativePath = relativePath;
    }

    public bool Overwrite { get; init; }
    public string RelativePath { get; }
}