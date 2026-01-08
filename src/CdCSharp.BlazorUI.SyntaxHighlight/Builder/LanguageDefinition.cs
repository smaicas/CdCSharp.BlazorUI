using CdCSharp.BlazorUI.SyntaxHighlight.Rules;
using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Builder;

public sealed class LanguageDefinition
{
    public string Name { get; }
    public bool CaseSensitive { get; }
    internal IReadOnlyList<ITokenRule> Rules { get; }

    internal LanguageDefinition(string name, bool caseSensitive, IReadOnlyList<ITokenRule> rules)
    {
        Name = name;
        CaseSensitive = caseSensitive;
        Rules = rules;
    }

    public static LanguageDefinitionBuilder Create(string name) => new(name);

    public IReadOnlyList<Token> Tokenize(string input)
    {
        TokenizerContext context = new()
        {
            LanguageName = Name,
            CaseSensitive = CaseSensitive
        };
        Tokenizer.Tokenizer tokenizer = new(Rules, context);
        return tokenizer.Tokenize(input);
    }
}
