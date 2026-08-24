using System;
using System.Globalization;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// The wire vocabulary shared with the client: lower-case enum names, and
    /// calendar days as bare "YYYY-MM-DD" strings.
    ///
    /// Calendar days are strings rather than DateTimes on purpose. A date
    /// serialised as an instant picks up the server's offset on the way out and
    /// the browser's on the way back, which moves the day the user picked —
    /// that is the bug this type exists to make impossible.
    /// </summary>
    public static class VacationWireFormat
    {
        public const string DayFormat = "yyyy-MM-dd";

        public static string ToDay(DateTime value)
        {
            return value.ToString(DayFormat, CultureInfo.InvariantCulture);
        }

        public static string ToDay(DateTime? value)
        {
            return value.HasValue ? ToDay(value.Value) : null;
        }

        public static DateTime? ParseDay(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Exact, invariant, date-only: anything with a time or an offset in
            // it is not a calendar day and must not be silently accepted.
            return DateTime.TryParseExact(
                value.Trim(),
                DayFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed)
                ? parsed.Date
                : null;
        }

        public static string TypeToWire(VacationRequestType type)
        {
            return type switch
            {
                VacationRequestType.Annual => "annual",
                VacationRequestType.Parental => "parental",
                VacationRequestType.Unpaid => "unpaid",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static VacationRequestType? ParseType(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "annual" => VacationRequestType.Annual,
                "parental" => VacationRequestType.Parental,
                "unpaid" => VacationRequestType.Unpaid,
                _ => null
            };
        }

        public static string StatusToWire(VacationRequestStatus status)
        {
            return status switch
            {
                VacationRequestStatus.Pending => "pending",
                VacationRequestStatus.Approved => "approved",
                VacationRequestStatus.Rejected => "rejected",
                VacationRequestStatus.Cancelled => "cancelled",
                _ => throw new ArgumentOutOfRangeException(nameof(status))
            };
        }

        public static VacationRequestStatus? ParseStatus(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "pending" => VacationRequestStatus.Pending,
                "approved" => VacationRequestStatus.Approved,
                "rejected" => VacationRequestStatus.Rejected,
                "cancelled" => VacationRequestStatus.Cancelled,
                _ => null
            };
        }

        public static string KindToWire(VacationEventKind kind)
        {
            return kind switch
            {
                VacationEventKind.Submitted => "submitted",
                VacationEventKind.Edited => "edited",
                VacationEventKind.Approved => "approved",
                VacationEventKind.Rejected => "rejected",
                VacationEventKind.Cancelled => "cancelled",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        public static VacationEventKind? ParseKind(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "submitted" => VacationEventKind.Submitted,
                "edited" => VacationEventKind.Edited,
                "approved" => VacationEventKind.Approved,
                "rejected" => VacationEventKind.Rejected,
                "cancelled" => VacationEventKind.Cancelled,
                _ => null
            };
        }

        /// <summary>
        /// The code payroll's monthly report uses for a leave type.
        /// A = atostogos, M = tėvadienis, NA = nemokamos atostogos.
        /// </summary>
        public static string TypeToReportLetter(VacationRequestType type)
        {
            return type switch
            {
                VacationRequestType.Annual => "A",
                VacationRequestType.Parental => "M",
                VacationRequestType.Unpaid => "NA",
                _ => "A"
            };
        }

        /// <summary>
        /// The code read back off a report, or the wire word, so a file edited
        /// by hand into "annual" still imports.
        /// </summary>
        public static VacationRequestType? ParseReportLetter(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "a" => VacationRequestType.Annual,
                "m" => VacationRequestType.Parental,
                "na" => VacationRequestType.Unpaid,
                var other => ParseType(other)
            };
        }
    }
}
