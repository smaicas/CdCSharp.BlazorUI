using CdCSharp.BlazorUI.Components.Layout.Modal;

namespace CdCSharp.BlazorUI.Components.Layout;

public enum ModalType
{
    Dialog,
    Drawer
}

public class ModalState
{
    public required string Id { get; init; }
    public required ModalType Type { get; init; }
    public required Type ComponentType { get; init; }
    public required ModalReference Reference { get; init; }
    public required ModalOptionsBase Options { get; init; }
    public Dictionary<string, object?>? Parameters { get; init; }
    public bool IsVisible { get; set; } = true;
    public bool IsAnimatingOut { get; set; }
}