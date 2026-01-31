namespace CdCSharp.BlazorUI.SyntaxHighlight;

public enum SyntaxHighlightLanguage
{
    CSharp,
    Razor,
    TypeScript,
    Css,
    Json
}

public static class SyntaxHighlightLanguageExtensions
{
    public static string ToLanguageIdentifier(this SyntaxHighlightLanguage language)
    {
        return language switch
        {
            SyntaxHighlightLanguage.CSharp => "csharp",
            SyntaxHighlightLanguage.Razor => "razor",
            SyntaxHighlightLanguage.TypeScript => "typescript",
            SyntaxHighlightLanguage.Css => "css",
            SyntaxHighlightLanguage.Json => "json",

            _ => throw new ArgumentOutOfRangeException(nameof(language))
        };
    }
}