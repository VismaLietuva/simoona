using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationCalculatorTests
    {
        // A Monday, so the ranges below are made of plain working days.
        private static readonly DateTime Monday = new DateTime(2026, 8, 17);

        [Test]
        public void CountWorkingDays_CountsBothEndsInclusive()
        {
            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(4), HolidayCalendar.Empty), Is.EqualTo(5));
        }

        [Test]
        public void CountWorkingDays_CountsASingleWorkingDayAsOne()
        {
            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday, HolidayCalendar.Empty), Is.EqualTo(1));
        }

        [Test]
        public void CountWorkingDays_ExcludesWeekends()
        {
            // Monday to the following Friday: two working weeks, no more.
            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(11), HolidayCalendar.Empty), Is.EqualTo(10));
        }

        [Test]
        public void CountWorkingDays_IsZeroForAPeriodEntirelyAtTheWeekend()
        {
            var saturday = Monday.AddDays(5);

            Assert.That(VacationCalculator.CountWorkingDays(saturday, saturday.AddDays(1), HolidayCalendar.Empty), Is.EqualTo(0));
        }

        [Test]
        public void CountWorkingDays_IsZeroForAReversedPeriod()
        {
            Assert.That(VacationCalculator.CountWorkingDays(Monday.AddDays(4), Monday, HolidayCalendar.Empty), Is.EqualTo(0));
        }

        [Test]
        public void CountWorkingDays_IgnoresAnyTimeComponent()
        {
            var late = Monday.AddHours(23).AddMinutes(59);

            Assert.That(VacationCalculator.CountWorkingDays(late, Monday.AddDays(4), HolidayCalendar.Empty), Is.EqualTo(5));
        }

        [Test]
        public void CountWorkingDays_AgreesWithADayByDayWalkOverALongPeriod()
        {
            // The whole-weeks shortcut is the only interesting thing in here, so
            // it is checked against the naive count it replaced, over every start
            // day of the week and a range of lengths.
            for (var offset = 0; offset < 7; offset++)
            {
                var start = Monday.AddDays(offset);
                for (var length = 0; length < 40; length++)
                {
                    var end = start.AddDays(length);
                    Assert.That(
                        VacationCalculator.CountWorkingDays(start, end, HolidayCalendar.Empty),
                        Is.EqualTo(NaiveWorkingDays(start, end)),
                        $"start {start:yyyy-MM-dd}, end {end:yyyy-MM-dd}");
                }
            }
        }

        [Test]
        public void CountWorkingDays_ExcludesAHolidayFallingOnAWorkingDay()
        {
            // The Wednesday of that Monday-to-Friday week.
            var holidays = Calendar("2026-08-19");

            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(4), holidays), Is.EqualTo(4));
        }

        [Test]
        public void CountWorkingDays_DoesNotSubtractAHolidayFallingAtTheWeekend()
        {
            // Already not a working day, so subtracting it would charge the week twice.
            var saturday = Monday.AddDays(5);

            Assert.That(
                VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(6), Calendar(saturday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))),
                Is.EqualTo(5));
        }

        [Test]
        public void CountWorkingDays_IsZeroForAPeriodThatIsNothingButAHoliday()
        {
            var holidays = Calendar("2026-08-17");

            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday, holidays), Is.EqualTo(0));
        }

        [Test]
        public void CountWorkingDays_IgnoresHolidaysOutsideThePeriod()
        {
            var holidays = Calendar("2026-08-14", "2026-08-24");

            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(4), holidays), Is.EqualTo(5));
        }

        [Test]
        public void CountWorkingDays_CountsHolidaysOnBothBoundsOfThePeriod()
        {
            // Inclusive at both ends: the first and last day of the period each
            // being a holiday must both come off.
            var holidays = Calendar("2026-08-17", "2026-08-21");

            Assert.That(VacationCalculator.CountWorkingDays(Monday, Monday.AddDays(4), holidays), Is.EqualTo(3));
        }

        [Test]
        public void CountWorkingDays_AgreesWithADayByDayWalkWhenHolidaysArePresent()
        {
            // The whole-weeks shortcut subtracts holidays in bulk, so it is checked
            // against the naive walk the same way the weekend logic is.
            var holidays = Calendar(
                "2026-08-19", "2026-08-22", "2026-08-31", "2026-09-08", "2026-09-20");

            for (var offset = 0; offset < 7; offset++)
            {
                var start = Monday.AddDays(offset);
                for (var length = 0; length < 40; length++)
                {
                    var end = start.AddDays(length);
                    Assert.That(
                        VacationCalculator.CountWorkingDays(start, end, holidays),
                        Is.EqualTo(NaiveWorkingDays(start, end, holidays)),
                        $"start {start:yyyy-MM-dd}, end {end:yyyy-MM-dd}");
                }
            }
        }

        /// <summary>
        /// Nobody is at the office on a holiday to sign the order, so it goes back
        /// past one the same way it goes back past a weekend.
        /// </summary>
        // Thursday back past a holiday Wednesday.
        [TestCase("2026-09-10", "2026-09-08", "2026-09-09")]
        // The same Thursday, with a holiday that is not in the way.
        [TestCase("2026-09-10", "2026-09-09", "2026-09-07")]
        // Monday back past a holiday Friday and the weekend behind it.
        [TestCase("2026-09-07", "2026-09-03", "2026-09-04")]
        public void PreviousWorkingDay_SkipsHolidaysAsWellAsWeekends(string leaveStarts, string expected, string holiday)
        {
            Assert.That(
                VacationCalculator.PreviousWorkingDay(
                    DateTime.Parse(leaveStarts, CultureInfo.InvariantCulture),
                    Calendar(holiday)),
                Is.EqualTo(DateTime.Parse(expected, CultureInfo.InvariantCulture)));
        }

        [Test]
        public void CommittedAnnualDays_DoesNotChargeAHolidayInsideTheLeave()
        {
            var requests = new[] { Annual(Monday, Monday.AddDays(4)) };

            Assert.That(
                VacationCalculator.CommittedAnnualDays(requests, null, Calendar("2026-08-19")),
                Is.EqualTo(4));
        }

        [Test]
        public void RangesOverlap_IsTrueWhenTheyShareASingleDay()
        {
            Assert.That(
                VacationCalculator.RangesOverlap(Monday, Monday.AddDays(4), Monday.AddDays(4), Monday.AddDays(8)),
                Is.True);
        }

        [Test]
        public void RangesOverlap_IsFalseForAdjacentPeriods()
        {
            Assert.That(
                VacationCalculator.RangesOverlap(Monday, Monday.AddDays(4), Monday.AddDays(5), Monday.AddDays(8)),
                Is.False);
        }

        [TestCase(0, 0)]
        [TestCase(9, 0)]
        [TestCase(10, 3)]
        [TestCase(14, 3)]
        [TestCase(15, 4)]
        [TestCase(20, 5)]
        [TestCase(25, 6)]
        [TestCase(40, 9)]
        public void SeniorityBonus_StepsEveryFiveYearsAfterTen(int yearsEmployed, double expected)
        {
            Assert.That(VacationCalculator.SeniorityBonus(yearsEmployed), Is.EqualTo(expected));
        }

        [Test]
        public void AnnualAccrual_AddsTheSeniorityBonusToTheStatutoryBase()
        {
            Assert.That(VacationCalculator.AnnualAccrual(9), Is.EqualTo(20));
            Assert.That(VacationCalculator.AnnualAccrual(15), Is.EqualTo(24));
            Assert.That(VacationCalculator.MonthlyAccrual(15), Is.EqualTo(2));
        }

        /// <summary>
        /// A leave order is signed at the office the working day before the leave
        /// starts, so a weekend pushes it further back than one day.
        /// </summary>
        [TestCase("2026-09-10", "2026-09-09")]
        [TestCase("2026-09-07", "2026-09-04")]
        [TestCase("2026-08-30", "2026-08-28")]
        [TestCase("2026-08-29", "2026-08-28")]
        [TestCase("2026-08-24", "2026-08-21")]
        public void PreviousWorkingDay_SkipsTheWeekend(string leaveStarts, string expected)
        {
            Assert.That(
                VacationCalculator.PreviousWorkingDay(DateTime.Parse(leaveStarts, CultureInfo.InvariantCulture), HolidayCalendar.Empty),
                Is.EqualTo(DateTime.Parse(expected, CultureInfo.InvariantCulture)));
        }

        [Test]
        public void AccruedBetween_IsZeroWhenTheEndIsNotAfterTheStart()
        {
            Assert.That(VacationCalculator.AccruedBetween(Monday, Monday, 20), Is.EqualTo(0));
            Assert.That(VacationCalculator.AccruedBetween(Monday, Monday.AddDays(-10), 20), Is.EqualTo(0));
        }

        [Test]
        public void ApproxAccruedNow_ReturnsTheEntitlementWhenNothingHasBeenImported()
        {
            Assert.That(VacationCalculator.ApproxAccruedNow(12.5, null, Monday, 20), Is.EqualTo(12.5));
        }

        [Test]
        public void CommittedAnnualDays_ChargesOnlyThePartAfterTheCutoff()
        {
            var cutoff = new DateTime(2026, 8, 19);
            // Monday to Friday, with the cutoff on the Wednesday: Thursday and
            // Friday are chargeable, the rest is already in the payslip figure.
            var requests = new[] { Annual(Monday, Monday.AddDays(4)) };

            Assert.That(VacationCalculator.CommittedAnnualDays(requests, cutoff, HolidayCalendar.Empty), Is.EqualTo(2));
        }

        [Test]
        public void CommittedAnnualDays_IgnoresLeaveEntirelyBeforeTheCutoff()
        {
            var requests = new[] { Annual(Monday, Monday.AddDays(4)) };

            Assert.That(VacationCalculator.CommittedAnnualDays(requests, Monday.AddDays(30), HolidayCalendar.Empty), Is.EqualTo(0));
        }

        [Test]
        public void CommittedAnnualDays_CountsEverythingWhenNoPayslipHasBeenImported()
        {
            var requests = new[] { Annual(Monday, Monday.AddDays(4)) };

            Assert.That(VacationCalculator.CommittedAnnualDays(requests, null, HolidayCalendar.Empty), Is.EqualTo(5));
        }

        [Test]
        public void CommittedAnnualDays_IgnoresRejectedCancelledAndNonAnnualLeave()
        {
            var requests = new List<VacationRequest>
            {
                Annual(Monday, Monday.AddDays(4), VacationRequestStatus.Rejected),
                Annual(Monday, Monday.AddDays(4), VacationRequestStatus.Cancelled),
                new VacationRequest
                {
                    Type = VacationRequestType.Unpaid,
                    Status = VacationRequestStatus.Approved,
                    DateFrom = Monday,
                    DateTo = Monday.AddDays(4)
                }
            };

            Assert.That(VacationCalculator.CommittedAnnualDays(requests, Monday.AddDays(-1), HolidayCalendar.Empty), Is.EqualTo(0));
        }

        [Test]
        public void CommittedAnnualDays_ExcludesTheRequestBeingEdited()
        {
            var requests = new[] { Annual(Monday, Monday.AddDays(4), id: 7) };

            Assert.That(
                VacationCalculator.CommittedAnnualDays(requests, Monday.AddDays(-1), HolidayCalendar.Empty, excludeRequestId: 7),
                Is.EqualTo(0));
        }

        private static VacationRequest Annual(
            DateTime from,
            DateTime to,
            VacationRequestStatus status = VacationRequestStatus.Approved,
            int id = 1)
        {
            return new VacationRequest
            {
                Id = id,
                Type = VacationRequestType.Annual,
                Status = status,
                DateFrom = from,
                DateTo = to
            };
        }

        private static int NaiveWorkingDays(DateTime from, DateTime to)
        {
            return NaiveWorkingDays(from, to, HolidayCalendar.Empty);
        }

        private static int NaiveWorkingDays(DateTime from, DateTime to, HolidayCalendar holidays)
        {
            var count = 0;
            for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Saturday
                    && day.DayOfWeek != DayOfWeek.Sunday
                    && !holidays.IsHoliday(day))
                {
                    count++;
                }
            }

            return count;
        }

        private static HolidayCalendar Calendar(params string[] days)
        {
            return new HolidayCalendar(
                Array.ConvertAll(days, day => DateTime.Parse(day, CultureInfo.InvariantCulture)));
        }
    }
}
