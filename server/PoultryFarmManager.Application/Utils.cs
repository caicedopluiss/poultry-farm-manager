using System;
using System.Globalization;

namespace PoultryFarmManager.Application;

public static class Utils
{
    public static DateTimeOffset ParseIso8601DateTimeString(string iso860DateTime)
    {
        return DateTimeOffset.ParseExact(iso860DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    public static bool IsIso8601DateStringValid(string iso8601DateTime) => DateTimeOffset.TryParseExact(iso8601DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    public static decimal TruncateToTwoDecimals(decimal value)
    {
        return Math.Truncate(value * 100) / 100;
    }
}