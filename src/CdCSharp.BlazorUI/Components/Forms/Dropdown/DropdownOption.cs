// File: Components/Forms/Dropdown/DropdownOption.cs
using CdCSharp.BlazorUI.Components.Forms.Dropdown;
using CdCSharp.BlazorUI.Core.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms;

public class DropdownOption<TOption> : ComponentBase, ISelectionOption, IDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [CascadingParameter] public IDropdownContainer? Container { get; set; }
    RenderFragment? ISelectionOption.Content => ChildContent ?? (builder => builder.AddContent(0, Text ?? Value?.ToString()));
    [Parameter] public bool Disabled { get; set; }
    string ISelectionOption.DisplayText => Text ?? Value?.ToString() ?? string.Empty;
    bool ISelectionOption.IsDisabled => Disabled;
    [Parameter] public string? Text { get; set; }
    [Parameter, EditorRequired] public TOption? Value { get; set; }
    object? ISelectionOption.Value => Value;

    public void Dispose()
    {
        Container?.UnregisterOption(this);
    }

    protected override void OnInitialized()
    {
        if (Container == null)
        {
            throw new InvalidOperationException($"{nameof(DropdownOption<TOption>)} must be used inside a BUIInputDropdown.");
        }

        ValidateOptionType();
        Container.RegisterOption(this);
    }

    private void ValidateOptionType()
    {
        Type optionType = typeof(TOption);
        Type expectedType = Container!.ElementType;

        if (!expectedType.IsAssignableFrom(optionType))
        {
            throw new InvalidOperationException(
                $"DropdownOption<{optionType.Name}> is not compatible with the dropdown's element type {expectedType.Name}. " +
                $"For single selection, use DropdownOption<{expectedType.Name}>. " +
                $"For multiple selection (arrays/lists), use DropdownOption<{expectedType.Name}> (the element type, not the collection type).");
        }
    }
}