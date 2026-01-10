using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Core.Components.Tree;

public abstract class TreeNodeComponentBase : ComponentBase
{
    private bool _registered;

    [CascadingParameter] internal ITreeRegistry? Registry { get; set; }
    [CascadingParameter(Name = "ParentNodeKey")] internal string? ParentNodeKey { get; set; }

    [Parameter] public string? Key { get; set; }
    [Parameter] public string? Text { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool InitiallyExpanded { get; set; }
    [Parameter] public RenderFragment? NodeContent { get; set; }
    [Parameter] public RenderFragment? ChildNodes { get; set; }

    protected string ResolvedKey { get; private set; } = string.Empty;

    protected override void OnInitialized()
    {
        ResolvedKey = Key ?? GenerateKey();
    }

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            Registry.RegisterNode(new TreeNodeRegistration
            {
                Key = ResolvedKey,
                Text = Text,
                Icon = Icon,
                Disabled = Disabled,
                InitiallyExpanded = InitiallyExpanded,
                Data = GetAdditionalData(),
                NodeContent = NodeContent,
                ParentKey = ParentNodeKey
            });

            _registered = true;
        }
    }

    protected virtual string GenerateKey() => $"node-{Guid.NewGuid():N}";

    protected abstract object? GetAdditionalData();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Registry != null && ChildNodes != null)
        {
            builder.OpenComponent<CascadingValue<string>>(0);
            builder.AddAttribute(1, "Value", ResolvedKey);
            builder.AddAttribute(2, "Name", "ParentNodeKey");
            builder.AddAttribute(3, "IsFixed", true);
            builder.AddAttribute(4, "ChildContent", ChildNodes);
            builder.CloseComponent();
        }
    }
}