using System.Text.RegularExpressions;

namespace CdCSharp.BlazorUI.Components.Utils;

public enum DateComponentType
{
    Day,
    Month,
    Year,
    Hour12,
    Hour24,
    Minute,
    Second,
    AmPm,
    Separator
}

public class DateComponent
{
    public DateComponentType Type { get; set; }
    public int MinDigits { get; set; }
    public int MaxDigits { get; set; }
    public string? SeparatorValue { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}

public class ParsedDatePattern
{
    public string RegexPattern { get; set; } = string.Empty;
    public List<DateComponent> Components { get; set; } = [];
    public string OriginalFormat { get; set; } = string.Empty;
}

public static class DatePatternParser
{
    private static readonly Dictionary<string, DateComponentType> ComponentMap = new()
    {
        { "d", DateComponentType.Day },
        { "dd", DateComponentType.Day },
        { "M", DateComponentType.Month },
        { "MM", DateComponentType.Month },
        { "yy", DateComponentType.Year },
        { "yyyy", DateComponentType.Year },
        { "h", DateComponentType.Hour12 },
        { "hh", DateComponentType.Hour12 },
        { "H", DateComponentType.Hour24 },
        { "HH", DateComponentType.Hour24 },
        { "m", DateComponentType.Minute },
        { "mm", DateComponentType.Minute },
        { "s", DateComponentType.Second },
        { "ss", DateComponentType.Second },
        { "t", DateComponentType.AmPm },
        { "tt", DateComponentType.AmPm }
    };

    public static ParsedDatePattern Parse(string dateFormat)
    {
        ParsedDatePattern result = new() { OriginalFormat = dateFormat };
        List<string> regexParts = [];
        List<DateComponent> components = [];

        // Parse character by character to handle single character formats
        int i = 0;
        while (i < dateFormat.Length)
        {
            char currentChar = dateFormat[i];

            // Check if it's a date/time format character
            if ("dMyhHmst".Contains(currentChar))
            {
                // Count consecutive same characters
                int count = 1;
                while (i + count < dateFormat.Length && dateFormat[i + count] == currentChar)
                {
                    count++;
                }

                string formatSpecifier = new(currentChar, count);
                DateComponent component = CreateDateComponent(formatSpecifier);
                components.Add(component);
                regexParts.Add(component.Pattern);

                i += count;
            }
            else
            {
                // It's a separator - collect all non-format characters
                int startIndex = i;
                while (i < dateFormat.Length && !"dMyhHmst".Contains(dateFormat[i]))
                {
                    i++;
                }

                string separator = dateFormat.Substring(startIndex, i - startIndex);
                components.Add(new DateComponent
                {
                    Type = DateComponentType.Separator,
                    SeparatorValue = separator,
                    Pattern = Regex.Escape(separator),
                    DefaultValue = separator
                });
                regexParts.Add(Regex.Escape(separator));
            }
        }

        result.Components = components;
        result.RegexPattern = string.Join("", regexParts);
        return result;
    }

    private static DateComponent CreateDateComponent(string formatSpecifier)
    {
        string firstChar = formatSpecifier[0].ToString();
        int length = formatSpecifier.Length;

        DateComponentType componentType = ComponentMap.ContainsKey(formatSpecifier)
            ? ComponentMap[formatSpecifier]
            : ComponentMap.ContainsKey(firstChar)
                ? ComponentMap[firstChar]
                : DateComponentType.Separator;

        DateComponent component = new()
        {
            Type = componentType,
            MinDigits = length,
            MaxDigits = length
        };

        // Handle variable length components
        switch (componentType)
        {
            case DateComponentType.Day:
            case DateComponentType.Month:
                if (length == 1)
                {
                    component.MinDigits = 1;
                    component.MaxDigits = 2;
                    component.Pattern = @"(\d{1,2})";
                }
                else
                {
                    component.Pattern = @"(\d{2})";
                }
                break;

            case DateComponentType.Year:
                component.Pattern = length == 2 ? @"(\d{2})" : @"(\d{4})";
                break;

            case DateComponentType.Hour12:
            case DateComponentType.Hour24:
            case DateComponentType.Minute:
            case DateComponentType.Second:
                if (length == 1)
                {
                    component.MinDigits = 1;
                    component.MaxDigits = 2;
                    component.Pattern = @"(\d{1,2})";
                }
                else
                {
                    component.Pattern = @"(\d{2})";
                }
                break;

            case DateComponentType.AmPm:
                component.Pattern = length == 1 ? @"([AP])" : @"(AM|PM)";
                component.MinDigits = length;
                component.MaxDigits = length == 1 ? 1 : 2;
                break;
        }

        // Set default values
        component.DefaultValue = GetDefaultValue(component);

        return component;
    }

    private static string GetDefaultValue(DateComponent component)
    {
        DateTime now = DateTime.Now;

        return component.Type switch
        {
            DateComponentType.Day => now.Day.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Month => now.Month.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Year => component.MinDigits == 2
                ? (now.Year % 100).ToString("00")
                : now.Year.ToString("0000"),
            DateComponentType.Hour12 => (now.Hour % 12 == 0 ? 12 : now.Hour % 12)
                .ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Hour24 => now.Hour.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Minute => now.Minute.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Second => now.Second.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.AmPm => component.MinDigits == 1
                ? (now.Hour < 12 ? "A" : "P")
                : (now.Hour < 12 ? "AM" : "PM"),
            DateComponentType.Separator => component.SeparatorValue ?? "",
            _ => ""
        };
    }

    public static List<ElementPattern> ConvertToElementPatterns(ParsedDatePattern parsedPattern, DateTime? currentValue = null)
    {
        List<ElementPattern> elements = [];
        DateTime dateValue = currentValue ?? DateTime.Now;
        DateTime defaultDateTime = DateTime.Now; // For default values

        foreach (DateComponent component in parsedPattern.Components)
        {
            string value = GetComponentValue(component, dateValue);
            string defaultValue = GetComponentValue(component, defaultDateTime);
            string pattern = GetElementPatternFormat(component);

            elements.Add(new ElementPattern(
                pattern: pattern,
                value: value,
                length: component.Type == DateComponentType.Separator ? component.SeparatorValue!.Length : component.MaxDigits,
                defaultValue: component.Type == DateComponentType.Separator ? component.SeparatorValue! : defaultValue,
                isSeparator: component.Type == DateComponentType.Separator,
                isEditable: component.Type != DateComponentType.Separator
            ));
        }

        return elements;
    }

    private static string GetComponentValue(DateComponent component, DateTime dateValue)
    {
        return component.Type switch
        {
            DateComponentType.Day => dateValue.Day.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Month => dateValue.Month.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Year => component.MinDigits == 2
                ? (dateValue.Year % 100).ToString("00")
                : dateValue.Year.ToString("0000"),
            DateComponentType.Hour12 => (dateValue.Hour % 12 == 0 ? 12 : dateValue.Hour % 12)
                .ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Hour24 => dateValue.Hour.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Minute => dateValue.Minute.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.Second => dateValue.Second.ToString().PadLeft(component.MinDigits, '0'),
            DateComponentType.AmPm => component.MinDigits == 1
                ? (dateValue.Hour < 12 ? "A" : "P")
                : (dateValue.Hour < 12 ? "AM" : "PM"),
            DateComponentType.Separator => component.SeparatorValue ?? "",
            _ => component.DefaultValue
        };
    }

    private static string GetElementPatternFormat(DateComponent component)
    {
        return component.Type switch
        {
            DateComponentType.Day => new string('d', component.MaxDigits),
            DateComponentType.Month => new string('d', component.MaxDigits),
            DateComponentType.Year => new string('d', component.MaxDigits),
            DateComponentType.Hour12 => new string('d', component.MaxDigits),
            DateComponentType.Hour24 => new string('d', component.MaxDigits),
            DateComponentType.Minute => new string('d', component.MaxDigits),
            DateComponentType.Second => new string('d', component.MaxDigits),
            DateComponentType.AmPm => new string('w', component.MaxDigits),
            DateComponentType.Separator => "", // Separators don't need a pattern
            _ => ""
        };
    }
}