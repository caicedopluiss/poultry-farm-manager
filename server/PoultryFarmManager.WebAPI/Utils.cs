using System;
using System.Linq;

namespace PoultryFarmManager.WebAPI;

internal static class Utils
{
    public static string BuildEndpointRoute(params string?[] segments)
    {
        char separator = '/';
        if (segments.Length == 0) return string.Empty;
        var parts = segments.SelectMany(segment => (segment ?? string.Empty).Split(separator, StringSplitOptions.RemoveEmptyEntries));
        return string.Join("/", parts.Where(segment => !string.IsNullOrWhiteSpace(segment)).Select(x => x!.Trim('/')).Distinct());
    }
}
