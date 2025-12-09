using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BlazorUI.Core.Theming.Themes;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace CdCSharp.BlazorUI.Core.Theming.Services;

public class ThemeService : IThemeService
{
    private readonly Dictionary<string, UITheme> _themes = [];
    private readonly IMemoryCache _cache;

    // Cache keys
    private const string CssStringCacheKey = "GeneratedThemeCss";
    private const string VariablesCacheKey = "AllCssVariables";

    public ThemeService(IMemoryCache cache)
    {
        _cache = cache;
        RegisterTheme(new LightTheme());
        RegisterTheme(new DarkTheme());
    }

    public void RegisterTheme(UITheme theme)
    {
        _themes[theme.Id] = theme;

        // Invalidate caches when a new theme is registered
        _cache.Remove(CssStringCacheKey);
        _cache.Remove(VariablesCacheKey);
    }

    /// <summary>
    /// Retrieves a dictionary containing all available CSS variable names and their corresponding values.
    /// </summary>
    /// <remarks>The returned dictionary is cached for subsequent calls to improve performance. Changes to the
    /// underlying CSS variables may not be reflected until the cache expires.</remarks>
    /// <returns>A dictionary where each key is a CSS variable name and each value is the associated CSS variable value. The
    /// dictionary will be empty if no CSS variables are defined.</returns>
    public Dictionary<string, string> GetCssVariables()
    {
        // Usamos TryGetValue para obtener o generar la caché
        if (!_cache.TryGetValue(VariablesCacheKey, out Dictionary<string, string>? variables))
        {
            // Si no está en caché, generar las variables
            variables = GetAllVariablesInternal();

            // Establecer la caché para las variables
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(VariablesCacheKey, variables, options);
        }

        return variables!;
    }

    /// <summary>
    /// Generates and returns the CSS string for the current theme, utilizing caching to improve performance.
    /// </summary>
    /// <remarks>The returned CSS is cached for up to 24 hours to reduce repeated computation. Calling this
    /// method multiple times within the cache duration will return the same CSS string unless the underlying theme data
    /// changes and the cache is invalidated.</remarks>
    /// <returns>A string containing the CSS for the current theme. Returns a cached value if available; otherwise, generates and
    /// caches a new CSS string.</returns>
    public string GenerateThemeCss()
    {
        if (_cache.TryGetValue(CssStringCacheKey, out string? cachedCss))
        {
            return cachedCss!;
        }

        string css = GenerateCssInternal();

        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(24));

        _cache.Set(CssStringCacheKey, css, cacheEntryOptions);

        return css;
    }

    private Dictionary<string, string> GetAllVariablesInternal()
    {
        Dictionary<string, string> allVariables = [];

        foreach (UITheme theme in _themes.Values)
        {
            // Fusion or override variables from each theme if there are duplicate keys
            foreach (KeyValuePair<string, string> variable in theme.GetCssVariables())
            {
                allVariables[variable.Key] = variable.Value;
            }
        }

        return allVariables;
    }

    private string GenerateCssInternal()
    {
        StringBuilder sb = new();

        // CSS Reset (Does not depend on variables, can be fixed)
        sb.AppendLine(CssReset.GetResetCss());
        sb.AppendLine();

        // Obtain variables (Ensure the dictionary is already cached)
        Dictionary<string, string> variables = GetCssVariables();

        // Write variables in :root {} format
        sb.AppendLine(":root {");
        foreach (KeyValuePair<string, string> variable in variables)
        {
            sb.AppendLine($"  {variable.Key}: {variable.Value};");
        }
        sb.AppendLine("}");

        return sb.ToString();
    }
}