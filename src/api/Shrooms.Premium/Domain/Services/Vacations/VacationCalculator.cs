using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Every figure the feature shows. The client mirrors this arithmetic in
    /// <c>src/lib/vacation-requests/</c>; the two must not be able to disagree.
    /// </summary>
    public static class VacationCalculator
    {
        public const double BaseDaysPerYear = 20;

        public const int SeniorityYears = 10;

        public const double SeniorityBaseBonus = 3;

        public const int SeniorityStepYears = 5;

        public const int MaxMonthsAhead = 12;

        private const double DaysPerYear = 365.25;

        /// <summary>
        /// Inclusive of both ends, weekends excluded. Public holidays deliberately
        /// are not: payroll reconciles them afterwards, so subtracting them here
        /// would only make the in-app figure disagree with the payslip.
        /// </summary>
        public static double CountWorkingDays(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date;
            if (end < start)
            {
                return 0;
            }

            var totalDays = (int)(end - start).TotalDays + 1;
            var wholeWeeks = totalDays / 7;
            var workingDays = wholeWeeks * 5;

            var cursor = start.AddDays(wholeWeeks * 7);
            while (cursor <= end)
            {
                if (cursor.DayOfWeek != DayOfWeek.Saturday && cursor.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }

                cursor = cursor.AddDays(1);
            }

            return workingDays;
        }

        /// <summary>
        /// The last working day before <paramref name="day"/>: an order is signed
        /// at the office before the leave starts, so Monday's is granted on Friday.
        /// </summary>
        public static DateTime PreviousWorkingDay(DateTime day)
        {
            var cursor = day.Date.AddDays(-1);
            while (cursor.DayOfWeek == DayOfWeek.Saturday || cursor.DayOfWeek == DayOfWeek.Sunday)
            {
                cursor = cursor.AddDays(-1);
            }

            return cursor;
        }

        public static bool RangesOverlap(DateTime aFrom, DateTime aTo, DateTime bFrom, DateTime bTo)
        {
            return aFrom.Date <= bTo.Date && bFrom.Date <= aTo.Date;
        }

        public static bool IsSingleDayType(VacationRequestType type)
        {
            return type == VacationRequestType.Parental;
        }

        public static bool DeductsBalance(VacationRequestType type)
        {
            return type == VacationRequestType.Annual;
        }

        public static bool IsActive(VacationRequestStatus status)
        {
            return status == VacationRequestStatus.Pending || status == VacationRequestStatus.Approved;
        }

        /// <summary>
        /// Extra days for length of service, as Lithuanian law steps them: three
        /// days from ten years, then one more every five — 3 at 10, 4 at 15,
        /// 5 at 20, 6 at 25, and so on.
        /// </summary>
        public static double SeniorityBonus(int yearsEmployed)
        {
            if (yearsEmployed < SeniorityYears)
            {
                return 0;
            }

            return SeniorityBaseBonus + ((yearsEmployed - SeniorityYears) / SeniorityStepYears);
        }

        public static double AnnualAccrual(int yearsEmployed)
        {
            return BaseDaysPerYear + SeniorityBonus(yearsEmployed);
        }

        public static double MonthlyAccrual(int yearsEmployed)
        {
            return AnnualAccrual(yearsEmployed) / 12;
        }

        public static double AccruedBetween(DateTime from, DateTime to, double annualDays)
        {
            var elapsed = (to.Date - from.Date).TotalDays;
            return elapsed <= 0 ? 0 : elapsed / DaysPerYear * annualDays;
        }

        /// <summary>
        /// Approximate by construction: it assumes an unbroken month at the
        /// contractual rate, which unpaid leave and mid-month starts both break.
        /// Never feed it into a rule — those use the exact payslip figure.
        /// </summary>
        public static double ApproxAccruedNow(double entitlement, DateTime? balanceAsOf, DateTime today, double annualDays)
        {
            if (balanceAsOf == null)
            {
                return Math.Round(entitlement, 1);
            }

            var earned = AccruedBetween(balanceAsOf.Value, today, annualDays);

            return Math.Round(entitlement + earned, 1);
        }

        /// <summary>
        /// Active annual leave falling *after* the payslip. The cutoff is the
        /// whole point: the entitlement already has earlier leave deducted, so
        /// counting a request from before it charges the employee twice. A period
        /// straddling the cutoff is charged only for the part after it.
        /// </summary>
        public static double CommittedAnnualDays(
            IEnumerable<VacationRequest> requests,
            DateTime? balanceAsOf,
            int? excludeRequestId = null)
        {
            // No payslip on file: nothing is netted out, so every active day counts.
            var firstChargeableDay = balanceAsOf?.Date.AddDays(1) ?? DateTime.MinValue;

            return requests
                .Where(request => request.Id != excludeRequestId
                                  && IsActive(request.Status)
                                  && DeductsBalance(request.Type))
                .Sum(request =>
                {
                    if (request.DateTo.Date < firstChargeableDay)
                    {
                        return 0;
                    }

                    var from = request.DateFrom.Date > firstChargeableDay ? request.DateFrom.Date : firstChargeableDay;
                    return CountWorkingDays(from, request.DateTo);
                });
        }

        /// <summary>Today in the organisation's zone — not UTC, which is a day behind late in the evening.</summary>
        public static DateTime TodayIn(string timeZoneId)
        {
            return NowIn(timeZoneId).Date;
        }

        /// <summary>
        /// The instant a local day starts, at that day's own offset: today's is
        /// wrong for a date the other side of a daylight saving change.
        /// </summary>
        public static DateTime ToUtcFrom(DateTime localDay, string timeZoneId)
        {
            var zone = ResolveTimeZone(timeZoneId);

            // Unspecified, or ConvertTimeToUtc refuses the kind it is handed.
            var local = DateTime.SpecifyKind(localDay, DateTimeKind.Unspecified);

            // A local time that does not exist (spring forward) is nudged on
            // rather than thrown over.
            return zone.IsInvalidTime(local)
                ? TimeZoneInfo.ConvertTimeToUtc(local.AddHours(1), zone)
                : TimeZoneInfo.ConvertTimeToUtc(local, zone);
        }

        public static DateTime NowIn(string timeZoneId)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ResolveTimeZone(timeZoneId));
        }

        private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
        {
            // Falls through rather than throwing: the default is a Windows id and
            // is not guaranteed to resolve everywhere the API can run.
            return Find(timeZoneId) ?? Find(DataLayerConstants.DefaultTimeZone) ?? TimeZoneInfo.Utc;
        }

        private static TimeZoneInfo Find(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                return null;
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
            {
                return null;
            }
        }
    }
}
