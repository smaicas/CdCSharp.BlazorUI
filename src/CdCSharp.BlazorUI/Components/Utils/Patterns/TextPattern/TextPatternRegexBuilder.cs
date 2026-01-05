using System.Text.RegularExpressions;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.TextPattern;

public static partial class TextPatternRegexBuilder
{
    public static string Build(string pattern)
    {
        string output = WordOrDigitGroup().Replace(pattern, "($1)");
        output = NotBetweenParentheses().Replace(output, "($1)");
        output = Word().Replace(output, @"\w");
        output = Digit().Replace(output, @"\d");
        return $"^{output}$";
    }

    [GeneratedRegex("[0-9]")]
    private static partial Regex Digit();

    [GeneratedRegex("[a-zA-Z]")]
    private static partial Regex Word();

    [GeneratedRegex("([a-zA-Z0-9]+)")]
    private static partial Regex WordOrDigitGroup();

    [GeneratedRegex(@"((?<!\([^)]*)[^()]+(?![^(]*\)))")]
    private static partial Regex NotBetweenParentheses();
}
