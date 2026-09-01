using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// The public holidays, as a set <see cref="VacationCalculator"/> can be handed.
    ///
    /// It is passed to the calculator rather than injected into it so the
    /// calculator stays static and free of I/O — the client mirrors that
    /// arithmetic, and arithmetic that reads a database cannot be mirrored.
    /// </summary>
    public sealed class HolidayCalendar
    {
        public static readonly HolidayCalendar Empty = new HolidayCalendar(Array.Empty<DateTime>());

        private readonly HashSet<DateTime> _days;

        public HolidayCalendar(IEnumerable<DateTime> days)
        {
            _days = new HashSet<DateTime>((days ?? Array.Empty<DateTime>()).Select(day => day.Date));
        }

        public bool IsHoliday(DateTime day)
        {
            return _days.Contains(day.Date);
        }

        /// <summary>
        /// Holidays in the period that would otherwise have been worked, both ends
        /// included. A holiday landing on a Saturday is skipped: the day is already
        /// not a working day, and counting it here would subtract it twice.
        /// </summary>
        public int CountWorkdayHolidaysBetween(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date;

            if (end < start || _days.Count == 0)
            {
                return 0;
            }

            // Walks the holidays, not the period: there are a handful a year
            // either way, but a decade-long range has thousands of days in it.
            return _days.Count(day => day >= start && day <= end && !IsWeekend(day));
        }

        private static bool IsWeekend(DateTime day)
        {
            return day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}
