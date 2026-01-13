using CdCSharp.BlazorUI.SyntaxHighlight.Rules;
using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Builder;

public sealed class LanguageDefinition
{
    internal LanguageDefinition(string name, bool caseSensitive, IReadOnlyList<ITokenRule> rules)
    {
        Name = name;
        CaseSensitive = caseSensitive;
        Rules = rules;
    }

    public bool CaseSensitive { get; }
    public string Name { get; }
    internal IReadOnlyList<ITokenRule> Rules { get; }

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