using System;
using System.Globalization;
using NUnit.Framework;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class HolidayCalendarTests
    {
        // Christmas and Boxing Day 2026 fall on a Friday and a Saturday, which is
        // the pair the weekend rule exists for.
        private static readonly HolidayCalendar Christmas = Calendar("2026-12-25", "2026-12-26");

        [Test]
        public void IsHoliday_IgnoresAnyTimeComponent()
        {
            Assert.That(Christmas.IsHoliday(Day("2026-12-25").AddHours(23).AddMinutes(59)), Is.True);
        }

        [Test]
        public void IsHoliday_IsFalseForAnOrdinaryDay()
        {
            Assert.That(Christmas.IsHoliday(Day("2026-12-24")), Is.False);
        }

        [Test]
        public void CountWorkdayHolidaysBetween_IncludesBothBounds()
        {
            Assert.That(Christmas.CountWorkdayHolidaysBetween(Day("2026-12-25"), Day("2026-12-25")), Is.EqualTo(1));
        }

        [Test]
        public void CountWorkdayHolidaysBetween_ExcludesAHolidayAtTheWeekend()
        {
            // Boxing Day is the Saturday, so the pair counts as one.
            Assert.That(Christmas.CountWorkdayHolidaysBetween(Day("2026-12-01"), Day("2026-12-31")), Is.EqualTo(1));
        }

        [Test]
        public void CountWorkdayHolidaysBetween_IsZeroJustOutsideTheHoliday()
        {
            Assert.That(Christmas.CountWorkdayHolidaysBetween(Day("2026-12-01"), Day("2026-12-24")), Is.EqualTo(0));
            Assert.That(Christmas.CountWorkdayHolidaysBetween(Day("2026-12-27"), Day("2026-12-31")), Is.EqualTo(0));
        }

        [Test]
        public void CountWorkdayHolidaysBetween_IsZeroForAReversedPeriod()
        {
            Assert.That(Christmas.CountWorkdayHolidaysBetween(Day("2026-12-31"), Day("2026-12-01")), Is.EqualTo(0));
        }

        [Test]
        public void CountWorkdayHolidaysBetween_IsZeroForAnEmptyCalendar()
        {
            Assert.That(
                HolidayCalendar.Empty.CountWorkdayHolidaysBetween(Day("2026-01-01"), Day("2040-12-31")),
                Is.EqualTo(0));
        }

        [Test]
        public void Constructor_ToleratesNullAndDeduplicates()
        {
            Assert.That(new HolidayCalendar(null).CountWorkdayHolidaysBetween(Day("2026-01-01"), Day("2026-12-31")), Is.EqualTo(0));

            var repeated = Calendar("2026-12-25", "2026-12-25");
            Assert.That(repeated.CountWorkdayHolidaysBetween(Day("2026-12-01"), Day("2026-12-31")), Is.EqualTo(1));
        }

        private static HolidayCalendar Calendar(params string[] days)
        {
            return new HolidayCalendar(Array.ConvertAll(days, Day));
        }

        private static DateTime Day(string value)
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
