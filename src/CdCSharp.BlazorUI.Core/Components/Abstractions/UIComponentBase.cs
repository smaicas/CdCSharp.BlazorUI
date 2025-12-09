using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public class UIComponentBase : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];
}
