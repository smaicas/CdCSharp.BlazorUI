using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Selection;

public interface ISelectionOption
{
    object? Value { get; }
    string DisplayText { get; }
    bool IsDisabled { get; }
    RenderFragment? Content { get; }
}

public interface IHierarchicalSelectionOption : ISelectionOption
{
    string Key { get; }
    int Depth { get; }
    bool HasChildren { get; }
    IHierarchicalSelectionOption? Parent { get; }
    IReadOnlyList<IHierarchicalSelectionOption> Children { get; }
}