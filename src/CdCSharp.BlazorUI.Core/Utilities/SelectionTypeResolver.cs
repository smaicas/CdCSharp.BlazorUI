using System.Collections;

public sealed class SelectionTypeResolver
{
    public bool IsMultiple { get; }
    public Type ElementType { get; }
    public Type ValueType { get; }

    public SelectionTypeResolver(Type valueType)
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
    }

    public IEnumerable<object> ExtractValues(object? value)
    {
        if (value == null) yield break;

        if (IsMultiple && value is IEnumerable enumerable and not string)
        {
            foreach (object item in enumerable)
            {
                yield return item;
            }
        }
        else if (!IsMultiple && value != null)
        {
            yield return value;
        }
    }

    public TValue CreateValue<TValue>(IEnumerable<object> values)
    {
        List<object> valueList = values.ToList();

        if (!IsMultiple)
        {
            return valueList.Count > 0 ? (TValue)valueList[0] : default!;
        }

        if (ValueType.IsArray)
        {
            Array array = Array.CreateInstance(ElementType, valueList.Count);
            for (int i = 0; i < valueList.Count; i++)
            {
                array.SetValue(valueList[i], i);
            }
            return (TValue)(object)array;
        }

        if (ValueType.IsGenericType)
        {
            Type genericDef = ValueType.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>))
            {
                IList list = (IList)Activator.CreateInstance(ValueType)!;
                foreach (object value in valueList)
                {
                    list.Add(value);
                }
                return (TValue)list;
            }

            if (genericDef == typeof(HashSet<>))
            {
                object set = Activator.CreateInstance(ValueType)!;
                System.Reflection.MethodInfo? addMethod = ValueType.GetMethod("Add");
                foreach (object value in valueList)
                {
                    addMethod?.Invoke(set, [value]);
                }
                return (TValue)set;
            }

            Array array = Array.CreateInstance(ElementType, valueList.Count);
            for (int i = 0; i < valueList.Count; i++)
            {
                array.SetValue(valueList[i], i);
            }
            return (TValue)(object)array;
        }

        return default!;
    }

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
        {
            return ValuesEqual(collection, value);
        }

        return ExtractValues(collection).Any(v => ValuesEqual(v, value));
    }
}