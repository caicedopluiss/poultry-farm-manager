using System;
using System.Globalization;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application;

internal static class Utils
{
    internal static DateTimeOffset ParseIso8601DateTimeString(string iso860DateTime)
    {
        return DateTimeOffset.ParseExact(iso860DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    internal static bool IsIso8601DateStringValid(string iso8601DateTime) => DateTimeOffset.TryParseExact(iso8601DateTime, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    internal static bool TryConvert(this UnitOfMeasure from, UnitOfMeasure to, decimal value, out decimal result)
    {
        try
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero.");
            result = (from, to) switch
            {
                // Weight conversions from Kilogram
                (UnitOfMeasure.Kilogram, UnitOfMeasure.Gram) => value * 1000m,
                (UnitOfMeasure.Kilogram, UnitOfMeasure.Pound) => value / 0.453592m,
                (UnitOfMeasure.Gram, UnitOfMeasure.Kilogram) => value / 1000m,
                (UnitOfMeasure.Gram, UnitOfMeasure.Pound) => value / 453.592m,
                (UnitOfMeasure.Pound, UnitOfMeasure.Kilogram) => value * 0.453592m,
                (UnitOfMeasure.Pound, UnitOfMeasure.Gram) => value * 453.592m,
                // Volume conversions from Liter
                (UnitOfMeasure.Liter, UnitOfMeasure.Milliliter) => value * 1000m,
                (UnitOfMeasure.Liter, UnitOfMeasure.Gallon) => value / 3.78541m,
                (UnitOfMeasure.Milliliter, UnitOfMeasure.Liter) => value / 1000m,
                (UnitOfMeasure.Milliliter, UnitOfMeasure.Gallon) => value / 3785.41m,
                (UnitOfMeasure.Gallon, UnitOfMeasure.Liter) => value * 3.78541m,
                (UnitOfMeasure.Gallon, UnitOfMeasure.Milliliter) => value * 3785.41m,

                // Count conversions (Unit and Piece are equivalent)
                (UnitOfMeasure.Unit, UnitOfMeasure.Piece) => value,
                (UnitOfMeasure.Piece, UnitOfMeasure.Unit) => value,

                (var _from, var _to) when _from == _to => value,

                _ => throw new ArgumentException($"Cannot convert from {from} to {to}")
            };
            return true;
        }
        catch
        {
            result = 0;
            return false;
        }
    }
}
