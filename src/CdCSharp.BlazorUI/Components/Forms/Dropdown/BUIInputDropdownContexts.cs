namespace CdCSharp.BlazorUI.Components.Forms.Dropdown;

public readonly record struct NoResultsContext(
    string SearchText,
    int TotalOptionsCount);