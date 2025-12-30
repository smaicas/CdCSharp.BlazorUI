namespace CdCSharp.BlazorUI.Core.Tokens;

public static class DesignTokens
{
    public static class Spacing
    {
        public const string Xs = "0.25rem";
        public const string Sm = "0.5rem";
        public const string Md = "1rem";
        public const string Lg = "1.5rem";
        public const string Xl = "2rem";
    }

    public static class FontSize
    {
        public const string Xs = "0.75rem";
        public const string Sm = "0.875rem";
        public const string Md = "1rem";
        public const string Lg = "1.125rem";
        public const string Xl = "1.25rem";
    }

    public static class LineHeight
    {
        public const string Tight = "1.25";
        public const string Normal = "1.5";
        public const string Relaxed = "1.75";
    }

    public static class BorderRadius
    {
        public const string None = "0";
        public const string Sm = "0.125rem";
        public const string Md = "0.25rem";
        public const string Lg = "0.5rem";
        public const string Full = "9999px";
    }

    public static class Transition
    {
        public const string Fast = "150ms";
        public const string Normal = "200ms";
        public const string Slow = "300ms";
        public const string Easing = "cubic-bezier(0.4, 0, 0.2, 1)";
    }

    public static class Elevation
    {
        public static readonly string[] Levels = new[]
        {
            "none",
            "0 1px 2px 0 rgba(0,0,0,0.05)",
            "0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px 0 rgba(0,0,0,0.06)",
            "0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -1px rgba(0,0,0,0.06)",
            "0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -2px rgba(0,0,0,0.05)",
            "0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04)",
            "0 25px 50px -12px rgba(0,0,0,0.25)"
        };
    }

    public static class ZIndex
    {
        public const int Base = 0;
        public const int Dropdown = 1000;
        public const int Sticky = 1100;
        public const int Fixed = 1200;
        public const int ModalBackdrop = 1300;
        public const int Modal = 1400;
        public const int Popover = 1500;
        public const int Tooltip = 1600;
    }
}
