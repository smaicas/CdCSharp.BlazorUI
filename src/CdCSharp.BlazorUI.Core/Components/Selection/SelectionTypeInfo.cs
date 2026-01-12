using System.Collections;

namespace CdCSharp.BlazorUI.Core.Components.Selection;

public sealed class SelectionTypeInfo
{
    public bool IsMultiple { get; }
    public Type ElementType { get; }
    public Type ValueType { get; }

    private readonly Func<object?, IEnumerable<object>> _extractValues;
    private readonly Func<IEnumerable<object>, object?> _createValue;

    public SelectionTypeInfo(Type valueType)
    {
        ValueType = valueType;

        if (valueType.IsArray)
        {
            IsMultiple = true;
            ElementType = valueType.GetElementType()!;
        }
        else if (valueType.IsGenericType &&
                 typeof(IEnumerable).IsAssignableFrom(valueType) &&
                 valueType != typeof(string))
        {
            IsMultiple = true;
            ElementType = valueType.GetGenericArguments()[0];
        }
        else
        {
            IsMultiple = false;
            ElementType = valueType;
        }

        _extractValues = BuildExtractValuesFunc();
        _createValue = BuildCreateValueFunc();
    }

    public IEnumerable<object> ExtractValues(object? value)
        => _extractValues(value);

    public TValue CreateValue<TValue>(IEnumerable<object> values)
        => (TValue)_createValue(values)!;

    public bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Equals(b);
    }

    public bool ContainsValue(object? collection, object? value)
    {
        if (collection == null || value == null) return false;

        if (!IsMultiple)
            return ValuesEqual(collection, value);

        return ExtractValues(collection).Any(v => ValuesEqual(v, value));
    }

    private Func<object?, IEnumerable<object>> BuildExtractValuesFunc()
    {
        return value =>
        {
            if (value == null)
                return Enumerable.Empty<object>();

            if (IsMultiple && value is IEnumerable enumerable and not string)
                return enumerable.Cast<object>();

            return new[] { value };
        };
    }

    private Func<IEnumerable<object>, object?> BuildCreateValueFunc()
    {
        if (!IsMultiple)
        {
            return values =>
            {
                List<object> list = values.ToList();
                return list.Count > 0 ? list[0] : null;
            };
        }

        if (ValueType.IsArray)
        {
            return values =>
            {
                List<object> list = values.ToList();
                Array array = Array.CreateInstance(ElementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                    array.SetValue(list[i], i);
                return array;
            };
        }

        if (ValueType.IsGenericType)
        {
            Type genericDef = ValueType.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>))
            {
                return values =>
                {
                    IList list = (IList)Activator.CreateInstance(ValueType)!;
                    foreach (object value in values)
                        list.Add(value);
                    return list;
                };
            }

            if (genericDef == typeof(HashSet<>))
            {
                System.Reflection.MethodInfo? addMethod = ValueType.GetMethod("Add");
                return values =>
                {
                    object set = Activator.CreateInstance(ValueType)!;
                    foreach (object value in values)
                        addMethod?.Invoke(set, new[] { value });
                    return set;
                };
            }
        }

        return values =>
        {
            List<object> list = values.ToList();
            Array array = Array.CreateInstance(ElementType, list.Count);
            for (int i = 0; i < list.Count; i++)
                array.SetValue(list[i], i);
            return array;
        };
    }
}