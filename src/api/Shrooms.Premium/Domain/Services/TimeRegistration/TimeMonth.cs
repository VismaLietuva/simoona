using System;
using System.Collections.Generic;
using System.Globalization;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Exceptions;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public static class TimeMonth
    {
        public const string MonthFormat = "yyyy-MM";
        public const string DayFormat = "yyyy-MM-dd";

        public static DateTime Parse(string value)
        {
            if (!DateTime.TryParseExact(value?.Trim(), MonthFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, "Invalid month");
            }

            return month.Date;
        }

        public static DateTime ParseDay(string value)
        {
            if (!DateTime.TryParseExact(value?.Trim(), DayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            {
                throw new ValidationException(ErrorCodes.TimeRegistrationInvalid, "Invalid date");
            }

            return day.Date;
        }

        public static string ToText(DateTime month)
        {
            return month.ToString(MonthFormat, CultureInfo.InvariantCulture);
        }

        public static string ToDay(DateTime day)
        {
            return day.ToString(DayFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime Of(DateTime day)
        {
            return new DateTime(day.Year, day.Month, 1);
        }

        public static DateTime LastDay(DateTime month)
        {
            return month.AddMonths(1).AddDays(-1);
        }

        public static IReadOnlyList<DateTime> Days(DateTime month)
        {
            var days = new List<DateTime>();
            for (var day = month; day.Month == month.Month; day = day.AddDays(1))
            {
                days.Add(day);
            }

            return days;
        }

        public static bool IsWeekend(DateTime day)
        {
            return day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
