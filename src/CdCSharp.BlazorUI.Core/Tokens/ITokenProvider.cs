namespace CdCSharp.BlazorUI.Core.Tokens;

public interface ITokenProvider
{
    string GetTokenValue(string tokenName);
    IDictionary<string, string> GetAllTokens();
    IDictionary<string, string> GetTokensByCategory(string category);
}
