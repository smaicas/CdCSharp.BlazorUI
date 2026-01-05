using System.Globalization;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.DateTimePattern;

public static class DateComponentValidator
{
    public static bool ValidateComponent(
        DateComponentType type,
        string value,
        ParsedDatePattern pattern,
        Dictionary<DateComponentType, string>? context)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return type switch
        {
            DateComponentType.Day => ValidateDay(value, context),
            DateComponentType.Month => ValidateMonth(value),
            DateComponentType.Year => ValidateYear(value),
            DateComponentType.Hour12 => ValidateHour12(value),
            DateComponentType.Hour24 => ValidateHour24(value),
            DateComponentType.Minute => ValidateMinute(value),
            DateComponentType.Second => ValidateSecond(value),
            DateComponentType.AmPm => ValidateAmPm(value),
            _ => true
        };
    }

    private static bool ValidateDay(string value, Dictionary<DateComponentType, string>? ctx)
    {
        if (!int.TryParse(value, out int day) || day is < 1 or > 31)
            return false;

        if (ctx != null &&
            ctx.TryGetValue(DateComponentType.Month, out string? m) &&
            int.TryParse(m, out int month))
        {
            int year = 2024;

            if (ctx.TryGetValue(DateComponentType.Year, out string? y) &&
                int.TryParse(y, out int parsedYear))
            {
                year = parsedYear < 100
                    ? ConvertTwoDigitYear(parsedYear)
                    : parsedYear;
            }

            return day <= DateTime.DaysInMonth(year, month);
        }

        return true;
    }

    private static bool ValidateMonth(string value)
        => int.TryParse(value, out int m) && m is >= 1 and <= 12;

    private static bool ValidateYear(string value)
        => int.TryParse(value, out int y) &&
           (value.Length <= 2 || y is >= 1900 and <= 2100);

    private static bool ValidateHour12(string value)
        => int.TryParse(value, out int h) && h is >= 1 and <= 12;

    private static bool ValidateHour24(string value)
        => int.TryParse(value, out int h) && h is >= 0 and <= 23;

    private static bool ValidateMinute(string value)
        => int.TryParse(value, out int m) && m is >= 0 and <= 59;

    private static bool ValidateSecond(string value)
        => int.TryParse(value, out int s) && s is >= 0 and <= 59;

    private static bool ValidateAmPm(string value)
        => value.ToUpperInvariant() is "A" or "P" or "AM" or "PM";

    private static int ConvertTwoDigitYear(int year)
        => CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);
}
