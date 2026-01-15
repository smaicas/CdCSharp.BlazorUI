namespace CdCSharp.BlazorUI.Components;

internal interface IBuiltComponent
{
    void BuildComponentCssVariables(Dictionary<string, string> cssVariables);

    void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes);
}