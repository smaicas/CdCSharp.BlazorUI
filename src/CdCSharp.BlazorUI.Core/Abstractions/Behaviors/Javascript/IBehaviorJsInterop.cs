using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;

public interface IBehaviorJsInterop
{
    ValueTask<IJSObjectReference> AttachBehaviorsAsync(BehaviorConfiguration configuration);
}

public class BehaviorConfiguration
{
    public RippleConfiguration? Ripple { get; set; }

    public bool HasAnyBehavior => Ripple != null;
}

public class RippleConfiguration
{
    public string? Color { get; set; }
    public int? Duration { get; set; }

    public ElementReference RippleContainer { get; set; }
}
