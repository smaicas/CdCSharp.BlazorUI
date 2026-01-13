namespace CdCSharp.BlazorUI.Components;

public enum FilterMode
{
    None,
    Contains,
    StartsWith,
    EndsWith,
    Equals,
    Custom
}

public enum SelectionMode
{
    None,
    Single,
    Multiple
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}

public enum TableVariant
{
    Default,
    Striped,
    Bordered,
    Cards,
}

public enum TableColumnAlign
{
    Left,
    Center,
    Right
}