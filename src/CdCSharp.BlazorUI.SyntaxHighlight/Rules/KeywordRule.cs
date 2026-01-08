using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Rules;

public sealed class KeywordRule : ITokenRule
{
    private readonly TokenType _tokenType;
    private readonly HashSet<string> _keywords;
    private readonly HashSet<string> _keywordsLower;
    private readonly int _minLength;
    private readonly int _maxLength;

    public int Priority { get; }

    public KeywordRule(TokenType tokenType, IEnumerable<string> keywords, int priority = 0)
    {
        _tokenType = tokenType;
        _keywords = [.. keywords];
        _keywordsLower = [.. _keywords.Select(k => k.ToLowerInvariant())];
        _minLength = _keywords.Min(k => k.Length);
        _maxLength = _keywords.Max(k => k.Length);
        Priority = priority;
    }

    public TokenMatch? TryMatch(string input, int position, TokenizerContext context)
    {
        if (position > 0 && IsWordChar(input[position - 1]))
            return null;

        int remainingLength = input.Length - position;
        if (remainingLength < _minLength)
            return null;

        int wordEnd = position;
        while (wordEnd < input.Length && IsWordChar(input[wordEnd]))
            wordEnd++;

        int wordLength = wordEnd - position;
        if (wordLength < _minLength || wordLength > _maxLength)
            return null;

        string word = input.Substring(position, wordLength);
        HashSet<string> keywordSet = context.CaseSensitive ? _keywords : _keywordsLower;
        string checkWord = context.CaseSensitive ? word : word.ToLowerInvariant();

        if (!keywordSet.Contains(checkWord))
            return null;

        return new TokenMatch(_tokenType, position, wordLength);
    }

    private static bool IsWordChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_';
}