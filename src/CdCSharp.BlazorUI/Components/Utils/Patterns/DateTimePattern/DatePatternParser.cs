using CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;
using System.Text.RegularExpressions;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.DateTimePattern;

public static class DatePatternParser
{
    private static readonly Dictionary<string, DateComponentType> ComponentMap = new()
    {
        { "d", DateComponentType.Day }, { "dd", DateComponentType.Day },
        { "M", DateComponentType.Month }, { "MM", DateComponentType.Month },
        { "yy", DateComponentType.Year }, { "yyyy", DateComponentType.Year },
        { "h", DateComponentType.Hour12 }, { "hh", DateComponentType.Hour12 },
        { "H", DateComponentType.Hour24 }, { "HH", DateComponentType.Hour24 },
        { "m", DateComponentType.Minute }, { "mm", DateComponentType.Minute },
        { "s", DateComponentType.Second }, { "ss", DateComponentType.Second },
        { "t", DateComponentType.AmPm }, { "tt", DateComponentType.AmPm }
    };

    public static ParsedDatePattern Parse(string format)
    {
        ParsedDatePattern parsed = new() { OriginalFormat = format };
        List<string> regexParts = [];

        int i = 0;
        while (i < format.Length)
        {
            char c = format[i];

            if ("dMyhHmst".Contains(c))
            {
                int count = 1;
                while (i + count < format.Length && format[i + count] == c)
                    count++;

                string specifier = new(c, count);
                DateComponent component = CreateComponent(specifier);

                parsed.Components.Add(component);
                regexParts.Add(component.Pattern);
                i += count;
            }
            else
            {
                int start = i;
                while (i < format.Length && !"dMyhHmst".Contains(format[i]))
                    i++;

                string sep = format.Substring(start, i - start);
                parsed.Components.Add(new DateComponent
                {
                    Type = DateComponentType.Separator,
                    SeparatorValue = sep,
                    Pattern = Regex.Escape(sep),
                    DefaultValue = sep
                });

                regexParts.Add(Regex.Escape(sep));
            }
        }

        parsed.RegexPattern = string.Join("", regexParts);
        return parsed;
    }

    public static List<ElementPattern> ConvertToElementPatterns(
        ParsedDatePattern parsed,
        DateTime? currentValue)
    {
        DateTime value = currentValue ?? DateTime.Now;
        DateTime defaults = DateTime.Now;

        List<ElementPattern> elements = [];

        foreach (DateComponent component in parsed.Components)
        {
            string val = GetValue(component, value);
            string def = GetValue(component, defaults);

            elements.Add(new ElementPattern(
                GetElementPattern(component),
                val,
                component.Type == DateComponentType.Separator
                    ? component.SeparatorValue!.Length
                    : component.MaxDigits,
                component.Type == DateComponentType.Separator
                    ? component.SeparatorValue!
                    : def,
                component.Type == DateComponentType.Separator,
                component.Type != DateComponentType.Separator
            ));
        }

        return elements;
    }

    private static DateComponent CreateComponent(string spec)
    {
        DateComponentType type = ComponentMap.TryGetValue(spec, out DateComponentType t)
            ? t
            : ComponentMap[spec[0].ToString()];

        DateComponent component = new()
        {
            Type = type,
            MinDigits = spec.Length,
            MaxDigits = spec.Length,
            Pattern = type switch
            {
                DateComponentType.Day or DateComponentType.Month
                    => spec.Length == 1 ? @"(\d{1,2})" : @"(\d{2})",

                DateComponentType.Year
                    => spec.Length == 2 ? @"(\d{2})" : @"(\d{4})",

                DateComponentType.Hour12 or DateComponentType.Hour24
                or DateComponentType.Minute or DateComponentType.Second
                    => spec.Length == 1 ? @"(\d{1,2})" : @"(\d{2})",

                DateComponentType.AmPm
                    => spec.Length == 1 ? @"([AP])" : @"(AM|PM)",

                _ => ""
            }
        };

        component.DefaultValue = GetValue(component, DateTime.Now);
        return component;
    }

    private static string GetValue(DateComponent component, DateTime date)
        => component.Type switch
        {
            DateComponentType.Day => date.Day.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Month => date.Month.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Year => component.MinDigits == 2
                ? (date.Year % 100).ToString("00")
                : date.Year.ToString("0000"),
            DateComponentType.Hour12 => ((date.Hour % 12 == 0 ? 12 : date.Hour % 12))
                .ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Hour24 => date.Hour.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Minute => date.Minute.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Second => date.Second.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.AmPm => component.MinDigits == 1
                ? (date.Hour < 12 ? "A" : "P")
                : (date.Hour < 12 ? "AM" : "PM"),
            DateComponentType.Separator => component.SeparatorValue ?? "",
            _ => ""
        };

    private static string GetElementPattern(DateComponent component)
        => component.Type switch
        {
            DateComponentType.AmPm => new string('w', component.MaxDigits),
            DateComponentType.Separator => "",
            _ => new string('d', component.MaxDigits)
        };
}
