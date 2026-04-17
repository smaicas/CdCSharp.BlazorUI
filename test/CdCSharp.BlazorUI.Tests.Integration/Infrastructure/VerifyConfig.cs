using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

public static class VerifyConfig
{
    private static readonly Regex ElementReferenceRegex =
        new(@"blazor:elementReference=""[a-f0-9]{8}(-[a-f0-9]{4}){3}-[a-f0-9]{12}""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OnClickRegex =
        new(@"blazor:(onclick|onchange|oninput|onfocus|onblur|onsubmit|onkeydown|onkeyup)=""\d+""",
            RegexOptions.Compiled);

    private static readonly Regex BuiGeneratedIdRegex =
        new(@"bui-(input|helper|checkbox|radio|switch|number|textarea|input-color|datetime)-[a-f0-9]{32}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PatternIdRegex =
        new(@"data-pattern-id=""pattern_[a-f0-9]{32}""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DropdownIdRegex =
        new(@"bui-dropdown-[a-f0-9]{32}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontScrubDateTimes();

        VerifierSettings.AddScrubber(sb =>
        {
            string text = sb.ToString();

            text = ElementReferenceRegex.Replace(
                text,
                @"blazor:elementReference=""<GUID>""");

            text = OnClickRegex.Replace(
                text,
                @"blazor:$1=""<EVENT>""");

            text = BuiGeneratedIdRegex.Replace(
                text,
                @"bui-$1-<ID>");

            text = PatternIdRegex.Replace(
                text,
                @"data-pattern-id=""<PATTERN_ID>""");

            text = DropdownIdRegex.Replace(
                text,
                @"bui-dropdown-<ID>");

            sb.Clear();
            sb.Append(text);
        });
    }
}