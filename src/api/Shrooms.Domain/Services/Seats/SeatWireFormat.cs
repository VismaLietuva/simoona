using System;
using System.Globalization;
using Shrooms.Contracts.Enums;

namespace Shrooms.Domain.Services.Seats
{
    public static class SeatWireFormat
    {
        public const string DayFormat = "yyyy-MM-dd";

        public static string ToDay(DateTime value)
        {
            return value.ToString(DayFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime? ParseDay(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTime.TryParseExact(
                value.Trim(),
                DayFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed)
                ? parsed.Date
                : null;
        }

        public static string ToWire(SeatType type)
        {
            return type == SeatType.Permanent ? "permanent" : "shared";
        }

        public static bool TryParseType(string value, out SeatType type)
        {
            if (string.Equals(value, "permanent", StringComparison.OrdinalIgnoreCase))
            {
                type = SeatType.Permanent;
                return true;
            }

            if (string.Equals(value, "shared", StringComparison.OrdinalIgnoreCase))
            {
                type = SeatType.Shared;
                return true;
            }

            type = SeatType.Shared;
            return false;
        }
    }
}
