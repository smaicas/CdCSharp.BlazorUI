using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Components;

public abstract class BUITreeNodeBase<TRegistration> : ComponentBase
    where TRegistration : TreeNodeRegistration
{
    private bool _registered;

    [CascadingParameter(Name = "TreeNodeRegistry")]
    internal ITreeNodeRegistry<TRegistration>? Registry { get; set; }

    [CascadingParameter(Name = "ParentNodeKey")]
    internal string? ParentNodeKey { get; set; }

    [Parameter] public string? Key { get; set; }
    [Parameter] public string? Text { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool InitiallyExpanded { get; set; }
    [Parameter] public object? Data { get; set; }
    [Parameter] public RenderFragment? NodeContent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected string ResolvedKey { get; private set; } = string.Empty;

    protected override void OnInitialized()
    {
        ResolvedKey = Key ?? GenerateDefaultKey();
    }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"BUITreeNodeBase.OnParametersSet - Key: {ResolvedKey}, Registry: {Registry != null}, _registered: {_registered}");

        if (Registry != null && !_registered)
        {
            TRegistration registration = CreateRegistration();
            Registry.Register(registration);
            _registered = true;
            Console.WriteLine($"Registrado: {ResolvedKey}");
        }
    }

    protected abstract string GenerateDefaultKey();
    protected abstract TRegistration CreateRegistration();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Registry != null && ChildContent != null)
        {
            builder.OpenComponent<CascadingValue<string>>(0);
            builder.AddComponentParameter(1, "Value", ResolvedKey);
            builder.AddComponentParameter(2, "Name", "ParentNodeKey");
            builder.AddComponentParameter(3, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }
}