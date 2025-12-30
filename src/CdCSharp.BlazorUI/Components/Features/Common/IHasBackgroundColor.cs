using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasBackgroundColor
{
    CssColor? BackgroundColor { get; set; }
}