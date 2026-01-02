using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;

public interface IJsBehavior
{
    ElementReference GetRootElement();
}
