using System;
using System.Globalization;

namespace PoultryFarmManager.Application;

internal static class Utils
{
    internal static DateTimeOffset ParseIso8601DateTimeString(string iso860DateTime)
    {
        return DateTimeOffset.ParseExact(iso860DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    internal static bool IsIso8601DateStringValid(string iso8601DateTime) => DateTimeOffset.TryParseExact(iso8601DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

}
