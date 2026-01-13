using CdCSharp.BlazorUI.SyntaxHighlight.Builder;
using CdCSharp.BlazorUI.SyntaxHighlight.Languages;
using CdCSharp.BlazorUI.SyntaxHighlight.Rendering;
using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight;

public sealed class Highlighter
{
    private readonly Dictionary<string, LanguageDefinition> _languages;
    private readonly HtmlRenderer _renderer;

    public Highlighter() : this(HtmlRenderOptions.Default)
    {
    }

    public Highlighter(HtmlRenderOptions options)
    {
        _renderer = new HtmlRenderer();
        Options = options;
        _languages = new Dictionary<string, LanguageDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["csharp"] = CSharpLanguage.Instance,
            ["cs"] = CSharpLanguage.Instance,
            ["c#"] = CSharpLanguage.Instance,
            ["razor"] = RazorLanguage.Instance,
            ["blazor"] = RazorLanguage.Instance,
            ["cshtml"] = RazorLanguage.Instance,
            ["typescript"] = TypeScriptLanguage.Instance,
            ["ts"] = TypeScriptLanguage.Instance,
            ["css"] = CssLanguage.Instance,
        };
    }

    public HtmlRenderOptions Options { get; set; }

    public IEnumerable<string> GetRegisteredLanguages() => _languages.Keys.Distinct();

    public bool HasLanguage(string name) => _languages.ContainsKey(name);

    public string Highlight(string language, string code)
    {
        if (string.IsNullOrEmpty(code))
            return string.Empty;

        LanguageDefinition definition = GetLanguage(language);
        IReadOnlyList<Token> tokens = definition.Tokenize(code);
        return _renderer.Render(tokens, Options);
    }

    public void RegisterLanguage(string name, LanguageDefinition definition)
    {
        _languages[name] = definition;
    }

    public void RegisterLanguage(string[] aliases, LanguageDefinition definition)
    {
        foreach (string alias in aliases)
        {
            _languages[alias] = definition;
        }
    }

    public IReadOnlyList<Token> Tokenize(string language, string code)
    {
        if (string.IsNullOrEmpty(code))
            return [];

        LanguageDefinition definition = GetLanguage(language);
        return definition.Tokenize(code);
    }

    private LanguageDefinition GetLanguage(string name)
    {
        if (_languages.TryGetValue(name, out LanguageDefinition? definition))
            return definition;

        throw new ArgumentException($"Language '{name}' is not registered. Available: {string.Join(", ", GetRegisteredLanguages())}", nameof(name));
    }
}