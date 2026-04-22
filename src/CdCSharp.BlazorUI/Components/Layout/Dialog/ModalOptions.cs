namespace CdCSharp.BlazorUI.Components.Layout;

public sealed class DialogOptions : ModalOptionsBase
{
    public bool FullScreen { get; set; }
    public string? MaxHeight { get; set; } = "90vh";
    public string? MaxWidth { get; set; } = "90vw";
    public string? MinHeight { get; set; }
    public string? MinWidth { get; set; } = "300px";
    public string? Title { get; set; }
}

public sealed class DrawerOptions : ModalOptionsBase
{
    public DrawerPosition Position { get; set; } = DrawerPosition.Right;
    public string Size { get; set; } = "300px";
}

public abstract class ModalOptionsBase
{
    public bool Closable { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public bool CloseOnOverlayClick { get; set; } = true;
    public string? CssClass { get; set; }
}

public enum DrawerPosition
{
    Left,
    Right,
    Top,
    Bottom
}