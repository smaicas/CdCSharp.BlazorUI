namespace CdCSharp.BlazorUI.Components;

public interface IHasError
{
    bool Error { get; }
    bool IsError { get; }
}