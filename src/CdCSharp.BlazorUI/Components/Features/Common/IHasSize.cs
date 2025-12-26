namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasSize<TSize> where TSize : Enum
{
    TSize Size { get; set; }
}