namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasSize
{
    SizeEnum Size { get; }
}
public enum SizeEnum
{
    Small,
    Medium,
    Large
}
