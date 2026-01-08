namespace CdCSharp.BlazorUI.Docs.Demo.Abstractions;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BuildComponentDemoAttribute : Attribute
{
    public string ComponentName { get; }

    public BuildComponentDemoAttribute(string componentName)
    {
        ComponentName = componentName;
    }
}