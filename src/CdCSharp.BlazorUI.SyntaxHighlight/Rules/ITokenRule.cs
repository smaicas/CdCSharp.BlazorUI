using CdCSharp.BlazorUI.SyntaxHighlight.Tokens;

namespace CdCSharp.BlazorUI.SyntaxHighlight.Rules;

public interface ITokenRule
{
    int Priority { get; }
    TokenMatch? TryMatch(string input, int position, TokenizerContext context);
}

public class TokenizerContext
{
    public string LanguageName { get; init; } = string.Empty;
    public bool CaseSensitive { get; init; } = true;
    public Stack<string> StateStack { get; } = new();

    public void PushState(string state) => StateStack.Push(state);
    public void PopState() => StateStack.TryPop(out _);
    public string? CurrentState => StateStack.TryPeek(out string? state) ? state : null;
}
