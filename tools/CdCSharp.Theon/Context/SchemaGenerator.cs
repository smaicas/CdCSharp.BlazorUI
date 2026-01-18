using CdCSharp.Theon.AI;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Context;

public static class SchemaGenerator
{
    public static ResponseFormat CreateResponseFormat<T>() where T : class
    {
        return new ResponseFormat
        {
            Type = "json_schema",
            JsonSchema = new JsonSchema
            {
                Name = typeof(T).Name.ToLowerInvariant(),
                Strict = "true",
                Schema = GenerateSchema(typeof(T))
            }
        };
    }

    private static object GenerateSchema(Type type)
    {
        Dictionary<string, object> schema = new()
        {
            ["type"] = "object",
            ["additionalProperties"] = false
        };

        Dictionary<string, object> properties = [];
        List<string> required = [];

        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo prop in props)
        {
            string propName = GetPropertyName(prop);
            properties[propName] = GetPropertySchema(prop.PropertyType);

            if (!IsNullable(prop))
            {
                required.Add(propName);
            }
        }

        schema["properties"] = properties;

        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        return schema;
    }

    private static string GetPropertyName(PropertyInfo prop)
    {
        JsonPropertyNameAttribute? attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
        return attr?.Name ?? ToCamelCase(prop.Name);
    }

    private static object GetPropertySchema(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string))
            return new { type = "string" };

        if (underlying == typeof(int) || underlying == typeof(long))
            return new { type = "integer" };

        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
            return new { type = "number" };

        if (underlying == typeof(bool))
            return new { type = "boolean" };

        if (underlying.IsArray || (underlying.IsGenericType && underlying.GetGenericTypeDefinition() == typeof(List<>)))
        {
            Type elementType = underlying.IsArray
                ? underlying.GetElementType()!
                : underlying.GetGenericArguments()[0];

            return new
            {
                type = "array",
                items = GetPropertySchema(elementType)
            };
        }

        if (underlying.IsClass && underlying != typeof(string))
        {
            return GenerateSchema(underlying);
        }

        return new { type = "string" };
    }

    private static bool IsNullable(PropertyInfo prop)
    {
        if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
            return true;

        NullabilityInfoContext context = new();
        NullabilityInfo info = context.Create(prop);
        return info.ReadState == NullabilityState.Nullable;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}