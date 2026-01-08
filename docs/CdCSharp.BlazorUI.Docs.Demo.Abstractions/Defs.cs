using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Docs.Demo.Abstractions;

public class ComponentDocDefinition
{
    public string Name { get; set; } = "";
    public List<string> Behaviors { get; set; } = [];
    public List<ParameterDefinition> Parameters { get; set; } = [];
    public List<ComponentDemoDefinition> Demos { get; set; } = [];
}

public class ParameterDefinition
{
    public string Name { get; }
    public Type Type { get; }
    public object? DefaultValue { get; }

    public ParameterDefinition(string name, Type type, object? defaultValue = null)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }
}

public class ComponentDemoDefinition
{
    public RenderFragment Demo { get; set; } = default!;
    public string Code { get; set; } = "";
}