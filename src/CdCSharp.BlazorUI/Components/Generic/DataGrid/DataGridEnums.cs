namespace CdCSharp.BlazorUI.Components;

public enum TableVariant
{
    Default,
    Striped,
    Bordered,
    Cards,
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

public enum FilterMode
{
    None,
    Contains,
    StartsWith,
    EndsWith,
    Equals,
    Custom
}

public enum TableColumnAlign
{
    Left,
    Center,
    Right
}