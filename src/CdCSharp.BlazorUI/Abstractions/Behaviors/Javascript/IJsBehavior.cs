using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Javascript;

public interface IJsBehavior
{
    ElementReference GetRootElement();
}
