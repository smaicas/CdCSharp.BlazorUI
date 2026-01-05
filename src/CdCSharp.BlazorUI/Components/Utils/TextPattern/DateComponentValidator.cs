using System.Globalization;

namespace CdCSharp.BlazorUI.Components.Utils;

public static class DateComponentValidator
{
    public static bool ValidateComponent(DateComponentType type, string value, ParsedDatePattern pattern, Dictionary<DateComponentType, string>? otherComponents = null)
    {
        if (string.IsNullOrEmpty(value)) return false;

        return type switch
        {
            DateComponentType.Day => ValidateDay(value, otherComponents),
            DateComponentType.Month => ValidateMonth(value),
            DateComponentType.Year => ValidateYear(value),
            DateComponentType.Hour12 => ValidateHour12(value),
            DateComponentType.Hour24 => ValidateHour24(value),
            DateComponentType.Minute => ValidateMinute(value),
            DateComponentType.Second => ValidateSecond(value),
            DateComponentType.AmPm => ValidateAmPm(value),
            DateComponentType.Separator => true,
            _ => false
        };
    }

    private static bool ValidateDay(string value, Dictionary<DateComponentType, string>? otherComponents)
    {
        if (!int.TryParse(value, out int day)) return false;
        if (day is < 1 or > 31) return false;

        // If we have month and year info, validate more precisely
        if (otherComponents != null)
        {
            if (otherComponents.TryGetValue(DateComponentType.Month, out string? monthStr) &&
                int.TryParse(monthStr, out int month))
            {
                if (month is < 1 or > 12) return true; // Month is invalid, can't validate day precisely

                int daysInMonth = DateTime.DaysInMonth(2024, month); // Use leap year for February

                if (otherComponents.TryGetValue(DateComponentType.Year, out string? yearStr) &&
                    int.TryParse(yearStr, out int year))
                {
                    // Handle 2-digit years
                    if (year < 100) year = ConvertTwoDigitYear(year);

                    try
                    {
                        daysInMonth = DateTime.DaysInMonth(year, month);
                    }
                    catch
                    {
                        // Invalid year, use default
                    }
                }

                return day <= daysInMonth;
            }
        }

        return true;
    }

    private static bool ValidateMonth(string value)
    {
        if (!int.TryParse(value, out int month)) return false;
        return month is >= 1 and <= 12;
    }

    private static bool ValidateYear(string value)
    {
        if (!int.TryParse(value, out int year)) return false;

        // 2-digit year
        if (value.Length <= 2)
        {
            return year is >= 0 and <= 99;
        }

        // 4-digit year
        return year is >= 1900 and <= 2100;
    }

    private static bool ValidateHour12(string value)
    {
        if (!int.TryParse(value, out int hour)) return false;
        return hour is >= 1 and <= 12;
    }

    private static bool ValidateHour24(string value)
    {
        if (!int.TryParse(value, out int hour)) return false;
        return hour is >= 0 and <= 23;
    }

    private static bool ValidateMinute(string value)
    {
        if (!int.TryParse(value, out int minute)) return false;
        return minute is >= 0 and <= 59;
    }

    private static bool ValidateSecond(string value)
    {
        if (!int.TryParse(value, out int second)) return false;
        return second is >= 0 and <= 59;
    }

    private static bool ValidateAmPm(string value)
    {
        string upperValue = value.ToUpperInvariant();
        return upperValue is "A" or "P" or
               "AM" or "PM";
    }

    public static int ConvertTwoDigitYear(int twoDigitYear)
    {
        // Use .NET's default two-digit year rule
        Calendar calendar = CultureInfo.CurrentCulture.Calendar;
        return calendar.ToFourDigitYear(twoDigitYear);
    }

    public static bool TryParseDateTime(ParsedDatePattern pattern, string input, out DateTime result)
    {
        result = default;

        try
        {
            // For now, use standard parsing with the original format
            // This could be enhanced to handle partial dates
            return DateTime.TryParseExact(input, pattern.OriginalFormat,
                CultureInfo.CurrentCulture, DateTimeStyles.None, out result);
        }
        catch
        {
            return false;
        }
    }

    public static Dictionary<DateComponentType, int> GetComponentIndices(ParsedDatePattern pattern)
    {
        Dictionary<DateComponentType, int> indices = new();
        int editableIndex = 0;

        foreach (DateComponent component in pattern.Components)
        {
            if (component.Type != DateComponentType.Separator &&
                !indices.ContainsKey(component.Type))
            {
                indices[component.Type] = editableIndex;
                editableIndex++;
            }
        }

        return indices;
    }
}