using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Transitions;

namespace CdCSharp.BlazorUI.Components.Layout;

public abstract class ModalOptionsBase
{
    public bool Closable { get; set; } = true;
    public bool CloseOnOverlayClick { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public BUITransitions? Transitions { get; set; }
    public string? CssClass { get; set; }
}

public class DialogOptions : ModalOptionsBase
{
    public string? Title { get; set; }
    public string? MinWidth { get; set; } = "300px";
    public string? MaxWidth { get; set; } = "90vw";
    public string? MinHeight { get; set; }
    public string? MaxHeight { get; set; } = "90vh";
    public bool FullScreen { get; set; }
}

public class DrawerOptions : ModalOptionsBase
{
    public DrawerPosition Position { get; set; } = DrawerPosition.Right;
    public string? Size { get; set; } = "300px";
}

public enum DrawerPosition
{
    Left,
    Right,
    Top,
    Bottom
}
