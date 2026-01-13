namespace CdCSharp.BuildTools.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class BuildToolsConfigurationAttribute : Attribute
{
    public string CssBundlePath { get; init; } = "CssBundle";
    public string TypesPath { get; init; } = "Types";
    public string WwwRootCssPath { get; init; } = "wwwroot/css";
    public string WwwRootJsPath { get; init; } = "wwwroot/js";
}