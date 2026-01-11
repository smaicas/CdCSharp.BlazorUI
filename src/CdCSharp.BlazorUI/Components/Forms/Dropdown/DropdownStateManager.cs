using CdCSharp.BlazorUI.Core.Utilities;

namespace CdCSharp.BlazorUI.Components.Forms.Dropdown;

public sealed class DropdownStateManager<TValue>
{
    private readonly SelectionTypeResolver _typeResolver;
    private readonly Action _onStateChanged;

    public bool IsOpen { get; private set; }
    public string SearchText { get; private set; } = string.Empty;
    public int FocusedIndex { get; private set; } = -1;

    public bool IsMultiple => _typeResolver.IsMultiple;
    public Type ElementType => _typeResolver.ElementType;

    public DropdownStateManager(Action onStateChanged)
    {
        _typeResolver = new SelectionTypeResolver(typeof(TValue));
        _onStateChanged = onStateChanged;
    }

    public void Open()
    {
        IsOpen = true;
        SearchText = string.Empty;
        FocusedIndex = -1;
        _onStateChanged();
    }

    public void Close()
    {
        IsOpen = false;
        _onStateChanged();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void SetSearchText(string text)
    {
        SearchText = text;
        FocusedIndex = -1;
        _onStateChanged();
    }

    public void MoveFocusUp(int itemCount)
    {
        FocusedIndex = Math.Max(FocusedIndex - 1, 0);
        _onStateChanged();
    }

    public void MoveFocusDown(int itemCount)
    {
        FocusedIndex = Math.Min(FocusedIndex + 1, itemCount - 1);
        _onStateChanged();
    }

    public void SetFocusToStart()
    {
        FocusedIndex = 0;
        _onStateChanged();
    }

    public void SetFocusToEnd(int itemCount)
    {
        FocusedIndex = itemCount - 1;
        _onStateChanged();
    }

    public IEnumerable<object> ExtractValues(TValue? value)
        => _typeResolver.ExtractValues(value);

    public TValue CreateValue(IEnumerable<object> values)
        => _typeResolver.CreateValue<TValue>(values);

    public bool ContainsValue(TValue? collection, object? value)
        => _typeResolver.ContainsValue(collection, value);

    public bool ValuesEqual(object? a, object? b)
        => _typeResolver.ValuesEqual(a, b);

    public TValue ToggleValue(TValue? currentValue, object? itemValue)
    {
        List<object> currentValues = ExtractValues(currentValue).ToList();

        if (IsMultiple)
        {
            if (currentValues.Any(v => ValuesEqual(v, itemValue)))
            {
                currentValues.RemoveAll(v => ValuesEqual(v, itemValue));
            }
            else if (itemValue != null)
            {
                currentValues.Add(itemValue);
            }
            return CreateValue(currentValues);
        }
        else
        {
            bool wasSelected = currentValues.Any(v => ValuesEqual(v, itemValue));
            return wasSelected ? default! : CreateValue(itemValue != null ? [itemValue] : []);
        }
    }

    public TValue SetSingleValue(object? itemValue)
    {
        return CreateValue(itemValue != null ? [itemValue] : []);
    }

    public TValue SelectAll(IEnumerable<object?> allValues)
    {
        if (!IsMultiple) return default!;
        return CreateValue(allValues.Where(v => v != null).Cast<object>());
    }

    public TValue DeselectAll()
    {
        return CreateValue([]);
    }
}
