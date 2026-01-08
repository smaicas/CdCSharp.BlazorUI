using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms.Radio;

public interface IRadioContainer
{
    Type ElementType { get; }
    void RegisterOption(IRadioOption option);
    void UnregisterOption(IRadioOption option);
    bool IsOptionSelected(object? value);
    Task SelectOptionAsync(object? value);
}

public interface IRadioOption
{
    object? RawValue { get; }
    bool IsDisabled { get; }
    RenderFragment? Content { get; }
}
