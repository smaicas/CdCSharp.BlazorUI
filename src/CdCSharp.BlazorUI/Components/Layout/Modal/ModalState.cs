namespace CdCSharp.BlazorUI.Components.Layout;

public enum ModalType
{
    Dialog,
    Drawer
}

public class ModalState
{
    public Type ComponentType { get; init; }
    public string Id { get; init; }
    public bool IsAnimatingOut { get; set; }
    public bool IsVisible { get; set; } = true;
    public ModalOptionsBase Options { get; init; }
    public Dictionary<string, object?>? Parameters { get; init; }
    public ModalReference Reference { get; init; }
    public ModalType Type { get; init; }
}