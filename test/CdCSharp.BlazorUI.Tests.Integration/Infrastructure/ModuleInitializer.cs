using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public static class ModuleInitializer
{
    static readonly Regex ElementReferenceRegex =
        new(@"blazor:elementReference=""[a-f0-9]{8}(-[a-f0-9]{4}){3}-[a-f0-9]{12}""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex OnClickRegex =
        new(@"blazor:(onclick|onchange|onsubmit)=""\d+""",
            RegexOptions.Compiled);

    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddScrubber(sb =>
        {
            string text = sb.ToString();

            text = ElementReferenceRegex.Replace(
                text,
                @"blazor:elementReference=""<GUID>""");

            text = OnClickRegex.Replace(
                text,
                @"blazor:$1=""<EVENT>""");

            sb.Clear();
            sb.Append(text);
        });
    }
}