using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Rules;

public interface ITokenRule
{
    int Priority { get; }

    TokenMatch? TryMatch(string input, int position, TokenizerContext context);
}

public class TokenizerContext
{
    public bool CaseSensitive { get; init; } = true;
    public string? CurrentState => StateStack.TryPeek(out string? state) ? state : null;
    public string LanguageName { get; init; } = string.Empty;
    public Stack<string> StateStack { get; } = new();

    public void PopState() => StateStack.TryPop(out _);

    public void PushState(string state) => StateStack.Push(state);
}