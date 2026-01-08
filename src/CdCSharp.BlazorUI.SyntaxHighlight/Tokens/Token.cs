namespace CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

public readonly record struct Token(
    TokenType Type,
    string Value,
    int StartIndex,
    int Length
)
{
    public int EndIndex => StartIndex + Length;

    public static Token Text(string value, int startIndex) =>
        new(TokenType.Text, value, startIndex, value.Length);
}