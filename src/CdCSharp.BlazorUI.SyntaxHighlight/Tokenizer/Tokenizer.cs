using CdCSharp.BlazorUI.SyntaxHighlight.Rules;
using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Tokenizer;

public sealed class Tokenizer
{
    private readonly TokenizerContext _context;
    private readonly List<ITokenRule> _rules;

    public Tokenizer(IEnumerable<ITokenRule> rules, TokenizerContext context)
    {
        _rules = [.. rules.OrderByDescending(r => r.Priority)];
        _context = context;
    }

    public IReadOnlyList<Token> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        List<Token> tokens = [];
        int position = 0;
        int textStart = 0;
        int lastPosition = -1;
        int stuckCount = 0;

        while (position < input.Length)
        {
            if (position == lastPosition)
            {
                stuckCount++;
                if (stuckCount > 10)
                {
                    position++;
                    textStart = position;
                    stuckCount = 0;
                    continue;
                }
            }
            else
            {
                lastPosition = position;
                stuckCount = 0;
            }

            TokenMatch? match = TryMatchRule(input, position);

            if (match.HasValue && match.Value.Length > 0)
            {
                if (position > textStart)
                {
                    string textValue = input.Substring(textStart, position - textStart);
                    tokens.Add(Token.Text(textValue, textStart));
                }

                if (match.Value.HasNestedTokens)
                {
                    foreach (Token nestedToken in match.Value.NestedTokens!)
                    {
                        tokens.Add(nestedToken with
                        {
                            StartIndex = nestedToken.StartIndex + match.Value.StartIndex
                        });
                    }
                }
                else
                {
                    string value = input.Substring(match.Value.StartIndex, match.Value.Length);
                    tokens.Add(new Token(match.Value.Type, value, match.Value.StartIndex, match.Value.Length));
                }

                position = match.Value.EndIndex;
                textStart = position;
            }
            else
            {
                position++;
            }
        }

        if (position > textStart)
        {
            string textValue = input.Substring(textStart, position - textStart);
            tokens.Add(Token.Text(textValue, textStart));
        }

        return tokens;
    }

    private TokenMatch? TryMatchRule(string input, int position)
    {
        foreach (ITokenRule rule in _rules)
        {
            try
            {
                TokenMatch? match = rule.TryMatch(input, position, _context);
                if (match.HasValue && match.Value.Length > 0)
                    return match;
            }
            catch
            {
                continue;
            }
        }
        return null;
    }
}