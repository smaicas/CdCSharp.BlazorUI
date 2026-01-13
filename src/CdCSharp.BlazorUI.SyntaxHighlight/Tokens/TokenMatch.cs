namespace CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

public readonly record struct TokenMatch(
    TokenType Type,
    int StartIndex,
    int Length,
    IReadOnlyList<Token>? NestedTokens = null
)
{
    public int EndIndex => StartIndex + Length;
    public bool HasNestedTokens => NestedTokens is { Count: > 0 };
}