using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Features.Behaviors;

public interface IJsBehavior
{
    ElementReference GetRootElement();
}
