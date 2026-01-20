namespace CdCSharp.Theon.Infrastructure;

public static class TokenEstimator
{
    private const double CharsPerToken = 4.2;

    public static int Estimate(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int nonWhitespace = text.Count(c => !char.IsWhiteSpace(c));
        int baseTokens = (int)(nonWhitespace / CharsPerToken);

        int structureTokens = text.Count(c => c is '{' or '}' or ';' or '(' or ')' or '[' or ']');
        return baseTokens + (structureTokens / 2);
    }

    public static int EstimateMessages(IEnumerable<string> messages)
    {
        return messages.Sum(Estimate);
    }

    //private static int EstimateTokens(string text)
    //{
    //    if (string.IsNullOrEmpty(text)) return 0;

    //    int tokens = 0;
    //    bool inWord = false;

    //    foreach (char c in text)
    //    {
    //        if (char.IsLetterOrDigit(c) || c == '_')
    //        {
    //            if (!inWord)
    //            {
    //                tokens++;
    //                inWord = true;
    //            }
    //        }
    //        else
    //        {
    //            inWord = false;
    //            if (!char.IsWhiteSpace(c))
    //            {
    //                tokens++;
    //            }
    //        }
    //    }

    //    return (int)(tokens * 1.25);
    //}
}
