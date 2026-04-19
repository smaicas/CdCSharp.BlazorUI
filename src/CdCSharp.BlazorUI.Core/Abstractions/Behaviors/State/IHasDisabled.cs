namespace CdCSharp.BlazorUI.Components;

public interface IHasDisabled
{
    public bool Disabled { get; set; }
    public bool IsDisabled { get; }
}