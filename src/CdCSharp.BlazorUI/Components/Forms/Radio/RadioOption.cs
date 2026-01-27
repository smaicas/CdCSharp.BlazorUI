using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms;

public class RadioOption<TOption> : ComponentBase, IRadioOption, IDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [CascadingParameter] public IRadioContainer? Container { get; set; }

    RenderFragment IRadioOption.Content => ChildContent ?? (builder => { });
    [Parameter] public bool Disabled { get; set; }
    bool IRadioOption.IsDisabled => Disabled;
    object? IRadioOption.RawValue => Value;
    [Parameter, EditorRequired] public TOption? Value { get; set; }

    public void Dispose()
    {
        Container?.UnregisterOption(this);
    }

    protected override void OnInitialized()
    {
        if (Container == null)
        {
            throw new InvalidOperationException(
                $"{nameof(RadioOption<TOption>)} must be used inside a BUIInputRadio.");
        }

        ValidateOptionType();
        Container.RegisterOption(this);
    }

    private void ValidateOptionType()
    {
        Type optionType = typeof(TOption);
        Type expectedType = Container!.ElementType;

        Type underlyingExpected = Nullable.GetUnderlyingType(expectedType) ?? expectedType;
        Type underlyingOption = Nullable.GetUnderlyingType(optionType) ?? optionType;

        if (!underlyingExpected.IsAssignableFrom(underlyingOption))
        {
            throw new InvalidOperationException(
                $"RadioOption<{optionType.Name}> is not compatible with the radio's value type {expectedType.Name}.");
        }
    }
}