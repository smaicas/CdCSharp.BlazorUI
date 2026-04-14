using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public interface IHasActive
{
    public bool Active { get; set; }
    public bool IsActive { get; }
}