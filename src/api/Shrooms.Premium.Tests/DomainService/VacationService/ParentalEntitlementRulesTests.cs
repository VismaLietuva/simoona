using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class ParentalEntitlementRulesTests
    {
        [Test]
        public void PeriodContaining_SpansTheCalendarMonth_LeapDayIncluded()
        {
            var (from, to) = ParentalEntitlementRules.PeriodContaining(ParentalPeriod.Month, new DateTime(2028, 2, 14));

            Assert.That(from, Is.EqualTo(new DateTime(2028, 2, 1)));
            Assert.That(to, Is.EqualTo(new DateTime(2028, 2, 29)));
        }

        [Test]
        public void PeriodContaining_SpansTheCalendarQuarter()
        {
            var (from, to) = ParentalEntitlementRules.PeriodContaining(ParentalPeriod.Quarter, new DateTime(2026, 9, 27));

            Assert.That(from, Is.EqualTo(new DateTime(2026, 7, 1)));
            Assert.That(to, Is.EqualTo(new DateTime(2026, 9, 30)));
        }

        [Test]
        public void PeriodContaining_RunsAWeekMondayToSunday()
        {
            foreach (var day in new[] { new DateTime(2026, 9, 28), new DateTime(2026, 10, 4) })
            {
                var (from, to) = ParentalEntitlementRules.PeriodContaining(ParentalPeriod.Week, day);

                Assert.That(from, Is.EqualTo(new DateTime(2026, 9, 28)), day.ToShortDateString());
                Assert.That(to, Is.EqualTo(new DateTime(2026, 10, 4)), day.ToShortDateString());
            }
        }

        [Test]
        public void EnsureAllowed_RefusesAnEmployeeWithNoEntitlement()
        {
            var error = Refused(null, new DateTime(2026, 8, 25));

            Assert.That(error.Code, Is.EqualTo("parentalNotEntitled"));
        }

        [Test]
        public void EnsureAllowed_RefusesADayToSomeoneOnShorterHours()
        {
            var error = Refused(ParentalEntitlementType.Hours2PerWeek, new DateTime(2026, 8, 25));

            Assert.That(error.Code, Is.EqualTo("parentalHoursOnly"));
        }

        [Test]
        public void EnsureAllowed_RefusesASecondDayInAMonthThatAllowsOne()
        {
            var error = Refused(
                ParentalEntitlementType.DayPerMonth,
                new DateTime(2026, 8, 25),
                ParentalDay(new DateTime(2026, 8, 18)));

            Assert.That(error.Code, Is.EqualTo("parentalLimit"));
            Assert.That(error.Parameters["booked"], Is.EqualTo(1d));
            Assert.That(error.Parameters["limit"], Is.EqualTo(1));
            Assert.That(error.Parameters["from"], Is.EqualTo("2026-08-01"));
            Assert.That(error.Parameters["to"], Is.EqualTo("2026-08-31"));
        }

        [Test]
        public void EnsureAllowed_MeasuresThePeriodTheChosenDayFallsIn()
        {
            Assert.DoesNotThrow(() => ParentalEntitlementRules.EnsureAllowed(
                ParentalEntitlementType.DayPerMonth,
                new DateTime(2026, 9, 7),
                1,
                new[] { ParentalDay(new DateTime(2026, 8, 18)) }));
        }

        [Test]
        public void EnsureAllowed_IgnoresWithdrawnAndRejectedDaysAndOtherLeave()
        {
            Assert.DoesNotThrow(() => ParentalEntitlementRules.EnsureAllowed(
                ParentalEntitlementType.DayPerMonth,
                new DateTime(2026, 8, 25),
                1,
                new[]
                {
                    ParentalDay(new DateTime(2026, 8, 18), VacationRequestStatus.Cancelled),
                    ParentalDay(new DateTime(2026, 8, 19), VacationRequestStatus.Rejected),
                    new VacationRequest
                    {
                        Type = VacationRequestType.Annual,
                        Status = VacationRequestStatus.Approved,
                        DateFrom = new DateTime(2026, 8, 20),
                        DateTo = new DateTime(2026, 8, 20),
                        WorkingDays = 1
                    }
                }));
        }

        [Test]
        public void EnsureAllowed_CountsPendingDaysAgainstTheLimit()
        {
            var error = Refused(
                ParentalEntitlementType.TwoDaysPerMonth,
                new DateTime(2026, 8, 27),
                ParentalDay(new DateTime(2026, 8, 18)),
                ParentalDay(new DateTime(2026, 8, 20), VacationRequestStatus.Pending));

            Assert.That(error.Code, Is.EqualTo("parentalLimit"));
        }

        [Test]
        public void EnsureAllowed_TreatsAQuarterAsOnePool()
        {
            var error = Refused(
                ParentalEntitlementType.DayPerQuarter,
                new DateTime(2026, 9, 14),
                ParentalDay(new DateTime(2026, 7, 6)));

            Assert.That(error.Code, Is.EqualTo("parentalLimit"));
        }

        [Test]
        public void Wire_RoundTripsEveryType()
        {
            foreach (ParentalEntitlementType type in Enum.GetValues(typeof(ParentalEntitlementType)))
            {
                Assert.That(ParentalEntitlementRules.ParseType(ParentalEntitlementRules.TypeToWire(type)), Is.EqualTo(type));
                Assert.That(() => ParentalEntitlementRules.RuleFor(type), Throws.Nothing);
            }

            Assert.That(ParentalEntitlementRules.ParseType("none"), Is.Null);
            Assert.That(ParentalEntitlementRules.ParseType("fiveDaysPerWeek"), Is.Null);
        }

        [Test]
        public void Balance_IsEmptyWithoutAnEntitlement()
        {
            var balance = ParentalEntitlementRules.Balance(null, new DateTime(2026, 11, 23), new[] { ParentalDay(new DateTime(2026, 11, 2)) });

            Assert.That(balance.Type, Is.Null);
            Assert.That(balance.Amount, Is.Null);
            Assert.That(balance.Booked, Is.Null);
            Assert.That(balance.Left, Is.Null);
        }

        [Test]
        public void Balance_CountsOnlyActiveDaysInThePeriod()
        {
            var balance = ParentalEntitlementRules.Balance(
                ParentalEntitlementType.DayPerQuarter,
                new DateTime(2026, 11, 23),
                new[]
                {
                    ParentalDay(new DateTime(2026, 10, 9)),
                    ParentalDay(new DateTime(2026, 9, 30)),
                    ParentalDay(new DateTime(2026, 11, 20), VacationRequestStatus.Cancelled)
                });

            Assert.That(balance.Type, Is.EqualTo("dayPerQuarter"));
            Assert.That(balance.Unit, Is.EqualTo("days"));
            Assert.That(balance.Amount, Is.EqualTo(1));
            Assert.That(balance.Period, Is.EqualTo("quarter"));
            Assert.That(balance.PeriodFrom, Is.EqualTo("2026-10-01"));
            Assert.That(balance.PeriodTo, Is.EqualTo("2026-12-31"));
            Assert.That(balance.Booked, Is.EqualTo(1));
            Assert.That(balance.Left, Is.EqualTo(0));
        }

        [Test]
        public void Balance_NeverGoesBelowZeroLeft()
        {
            var balance = ParentalEntitlementRules.Balance(
                ParentalEntitlementType.DayPerMonth,
                new DateTime(2026, 11, 23),
                new[] { ParentalDay(new DateTime(2026, 11, 2)), ParentalDay(new DateTime(2026, 11, 9)) });

            Assert.That(balance.Booked, Is.EqualTo(2));
            Assert.That(balance.Left, Is.EqualTo(0));
        }

        [Test]
        public void Balance_HasNoBookedOrLeftForShorterHours()
        {
            var balance = ParentalEntitlementRules.Balance(
                ParentalEntitlementType.Hours2PerWeek,
                new DateTime(2026, 11, 23),
                new[] { ParentalDay(new DateTime(2026, 11, 23)) });

            Assert.That(balance.Unit, Is.EqualTo("hours"));
            Assert.That(balance.Amount, Is.EqualTo(2));
            Assert.That(balance.Period, Is.EqualTo("week"));
            Assert.That(balance.Booked, Is.Null);
            Assert.That(balance.Left, Is.Null);
        }

        [Test]
        public void BalanceBefore_LeavesTheRequestItselfOut()
        {
            var request = ParentalDay(new DateTime(2026, 11, 10), VacationRequestStatus.Pending, id: 2);
            var earlier = ParentalDay(new DateTime(2026, 11, 3), id: 1);

            var balance = ParentalEntitlementRules.BalanceBefore(
                request,
                ParentalEntitlementType.TwoDaysPerMonth,
                new[] { earlier, request });

            Assert.That(balance.Booked, Is.EqualTo(1));
            Assert.That(balance.Left, Is.EqualTo(1));
        }

        [Test]
        public void BalanceBefore_MeasuresThePeriodTheRequestStartsIn()
        {
            var request = ParentalDay(new DateTime(2026, 12, 1), VacationRequestStatus.Pending, id: 2);

            var balance = ParentalEntitlementRules.BalanceBefore(
                request,
                ParentalEntitlementType.DayPerMonth,
                new[] { ParentalDay(new DateTime(2026, 11, 30), id: 1), request });

            Assert.That(balance.PeriodFrom, Is.EqualTo("2026-12-01"));
            Assert.That(balance.PeriodTo, Is.EqualTo("2026-12-31"));
            Assert.That(balance.Booked, Is.EqualTo(0));
            Assert.That(balance.Left, Is.EqualTo(1));
        }

        private static VacationValidationException Refused(
            ParentalEntitlementType? entitlement,
            DateTime day,
            params VacationRequest[] ownRequests)
        {
            return Assert.Throws<VacationValidationException>(() =>
                ParentalEntitlementRules.EnsureAllowed(entitlement, day, 1, new List<VacationRequest>(ownRequests)));
        }

        private static VacationRequest ParentalDay(DateTime day, VacationRequestStatus status = VacationRequestStatus.Approved, int id = 0)
        {
            return new VacationRequest
            {
                Id = id,
                Type = VacationRequestType.Parental,
                Status = status,
                DateFrom = day,
                DateTo = day,
                WorkingDays = 1
            };
        }
    }
}
