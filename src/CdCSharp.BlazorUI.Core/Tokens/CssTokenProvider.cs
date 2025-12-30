using System.Reflection;

namespace CdCSharp.BlazorUI.Core.Tokens;

public class CssTokenProvider : ITokenProvider
{
    private readonly Dictionary<string, string> _tokens = [];

    public CssTokenProvider()
    {
        LoadTokens();
    }

    public string GetTokenValue(string tokenName) =>
        _tokens.TryGetValue(tokenName, out string? value) ? value : string.Empty;

    public IDictionary<string, string> GetAllTokens() =>
        new Dictionary<string, string>(_tokens);

    public IDictionary<string, string> GetTokensByCategory(string category) =>
        _tokens.Where(kvp => kvp.Key.StartsWith(category, StringComparison.OrdinalIgnoreCase))
               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    private void LoadTokens()
    {
        Type[] tokenTypes = typeof(DesignTokens).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (Type type in tokenTypes)
        {
            string category = type.Name.ToLowerInvariant();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    string key = $"{category}-{field.Name.ToLowerInvariant()}";
                    string value = field.GetValue(null)?.ToString() ?? string.Empty;
                    _tokens[key] = value;
                }
                else if (field.FieldType == typeof(string[]))
                {
                    if (field.GetValue(null) is string[] values)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            string key = $"{category}-{field.Name.ToLowerInvariant()}-{i}";
                            _tokens[key] = values[i];
                        }
                    }
                }
            }
        }
    }
}
