using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public enum ParentalUnit
    {
        Days,
        Hours
    }

    public enum ParentalPeriod
    {
        Week,
        Month,
        Quarter
    }

    public sealed class ParentalRule
    {
        public ParentalRule(ParentalUnit unit, int amount, ParentalPeriod period)
        {
            Unit = unit;
            Amount = amount;
            Period = period;
        }

        public ParentalUnit Unit { get; }

        public int Amount { get; }

        public ParentalPeriod Period { get; }
    }

    // DK art. 138: unused parental time does not carry over, so each rule is measured within one calendar period.
    public static class ParentalEntitlementRules
    {
        private static readonly IReadOnlyDictionary<ParentalEntitlementType, ParentalRule> Rules =
            new Dictionary<ParentalEntitlementType, ParentalRule>
            {
                [ParentalEntitlementType.DayPerQuarter] = new ParentalRule(ParentalUnit.Days, 1, ParentalPeriod.Quarter),
                [ParentalEntitlementType.DayPerMonth] = new ParentalRule(ParentalUnit.Days, 1, ParentalPeriod.Month),
                [ParentalEntitlementType.TwoDaysPerMonth] = new ParentalRule(ParentalUnit.Days, 2, ParentalPeriod.Month),
                [ParentalEntitlementType.Hours2PerMonth] = new ParentalRule(ParentalUnit.Hours, 2, ParentalPeriod.Month),
                [ParentalEntitlementType.Hours2PerWeek] = new ParentalRule(ParentalUnit.Hours, 2, ParentalPeriod.Week),
                [ParentalEntitlementType.Hours4PerWeek] = new ParentalRule(ParentalUnit.Hours, 4, ParentalPeriod.Week)
            };

        public static ParentalRule RuleFor(ParentalEntitlementType type)
        {
            return Rules[type];
        }

        public static (DateTime From, DateTime To) PeriodContaining(ParentalPeriod period, DateTime day)
        {
            var date = day.Date;
            switch (period)
            {
                case ParentalPeriod.Month:
                {
                    var from = new DateTime(date.Year, date.Month, 1);
                    return (from, from.AddMonths(1).AddDays(-1));
                }

                case ParentalPeriod.Quarter:
                {
                    var from = new DateTime(date.Year, ((date.Month - 1) / 3 * 3) + 1, 1);
                    return (from, from.AddMonths(3).AddDays(-1));
                }

                default:
                {
                    var fromMonday = ((int)date.DayOfWeek + 6) % 7;
                    var from = date.AddDays(-fromMonday);
                    return (from, from.AddDays(6));
                }
            }
        }

        public static double BookedDays(IEnumerable<VacationRequest> ownRequests, ParentalRule rule, DateTime day)
        {
            var (from, to) = PeriodContaining(rule.Period, day);

            return ownRequests
                .Where(request => request.Type == VacationRequestType.Parental
                                  && VacationCalculator.IsActive(request.Status)
                                  && request.DateFrom.Date >= from
                                  && request.DateFrom.Date <= to)
                .Sum(request => request.WorkingDays);
        }

        public static void EnsureAllowed(
            ParentalEntitlementType? entitlement,
            DateTime dateFrom,
            double requestedDays,
            IEnumerable<VacationRequest> ownRequests)
        {
            if (entitlement == null)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationParentalNotEntitled,
                    "parentalNotEntitled",
                    Resx.GetResourceString("parentalNotEntitled"));
            }

            var rule = RuleFor(entitlement.Value);
            if (rule.Unit == ParentalUnit.Hours)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationParentalHoursOnly,
                    "parentalHoursOnly",
                    Resx.GetResourceString("parentalHoursOnly"));
            }

            var booked = BookedDays(ownRequests, rule, dateFrom);
            if (booked + requestedDays <= rule.Amount)
            {
                return;
            }

            var (from, to) = PeriodContaining(rule.Period, dateFrom);
            throw VacationRequestValidator.Fail(
                ErrorCodes.VacationParentalLimit,
                "parentalLimit",
                Resx.GetResourceString(
                    "parentalLimit",
                    booked,
                    rule.Amount,
                    VacationWireFormat.ToDay(from),
                    VacationWireFormat.ToDay(to)),
                new Dictionary<string, object>
                {
                    ["booked"] = booked,
                    ["limit"] = rule.Amount,
                    ["from"] = VacationWireFormat.ToDay(from),
                    ["to"] = VacationWireFormat.ToDay(to)
                });
        }

        public static string TypeToWire(ParentalEntitlementType type)
        {
            return type switch
            {
                ParentalEntitlementType.DayPerQuarter => "dayPerQuarter",
                ParentalEntitlementType.DayPerMonth => "dayPerMonth",
                ParentalEntitlementType.TwoDaysPerMonth => "twoDaysPerMonth",
                ParentalEntitlementType.Hours2PerMonth => "hours2PerMonth",
                ParentalEntitlementType.Hours2PerWeek => "hours2PerWeek",
                ParentalEntitlementType.Hours4PerWeek => "hours4PerWeek",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static ParentalEntitlementType? ParseType(string value)
        {
            return (value ?? string.Empty).Trim() switch
            {
                "dayPerQuarter" => ParentalEntitlementType.DayPerQuarter,
                "dayPerMonth" => ParentalEntitlementType.DayPerMonth,
                "twoDaysPerMonth" => ParentalEntitlementType.TwoDaysPerMonth,
                "hours2PerMonth" => ParentalEntitlementType.Hours2PerMonth,
                "hours2PerWeek" => ParentalEntitlementType.Hours2PerWeek,
                "hours4PerWeek" => ParentalEntitlementType.Hours4PerWeek,
                _ => null
            };
        }

        public static string UnitToWire(ParentalUnit unit)
        {
            return unit == ParentalUnit.Days ? "days" : "hours";
        }

        public static string PeriodToWire(ParentalPeriod period)
        {
            return period switch
            {
                ParentalPeriod.Week => "week",
                ParentalPeriod.Month => "month",
                _ => "quarter"
            };
        }
    }
}
