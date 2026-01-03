namespace CdCSharp.BuildTools.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class BuildTemplateAttribute : Attribute
{
    public string RelativePath { get; }
    public bool Overwrite { get; init; }

    public BuildTemplateAttribute(string relativePath)
    {
        RelativePath = relativePath;
    }
}
