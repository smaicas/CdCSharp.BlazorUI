using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Rules;

public sealed class SequenceRule : ITokenRule
{
    private readonly TokenType _tokenType;
    private readonly string _sequence;

    public int Priority { get; }

    public SequenceRule(TokenType tokenType, string sequence, int priority = 0)
    {
        _tokenType = tokenType;
        _sequence = sequence;
        Priority = priority;
    }

    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        StringComparison comparison = context.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if (input.AsSpan(position).StartsWith(_sequence, comparison))
            return new TokenMatch(_tokenType, position, _sequence.Length);

        return null;
    }
}
