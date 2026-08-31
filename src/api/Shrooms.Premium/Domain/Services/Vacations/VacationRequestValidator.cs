using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Separate from the service that persists, so the rules can be read and
    /// tested without a database. The client enforces the same set before
    /// submitting; this is the enforcement that counts.
    /// </summary>
    public static class VacationRequestValidator
    {
        public const int MinRejectReasonLength = 3;

        public static DateTime EffectiveDateTo(VacationRequestType type, DateTime dateFrom, DateTime? dateTo)
        {
            return VacationCalculator.IsSingleDayType(type) ? dateFrom : (dateTo ?? dateFrom);
        }

        /// <summary>
        /// The request being edited must already be excluded from
        /// <paramref name="ownRequests"/>, or it always clashes with itself.
        /// </summary>
        public static void ValidateDraft(
            VacationRequestType type,
            DateTime dateFrom,
            DateTime dateTo,
            string note,
            DateTime today,
            IEnumerable<VacationRequest> ownRequests,
            DateTime? originalDateFrom = null)
        {
            if (dateTo < dateFrom)
            {
                throw Fail(ErrorCodes.VacationWrongPeriod, "wrongPeriod", Resx.GetResourceString("wrongVacationPeriod"));
            }

            // Only a start the employee actually moves has to be in the future.
            // Leaving an already-started one where it is has to stay allowed, or
            // someone cutting short the leave they are on could never save.
            if (dateFrom < today && dateFrom.Date != originalDateFrom?.Date)
            {
                throw Fail(ErrorCodes.VacationStartInPast, "startInPast", Resx.GetResourceString("startInPast"));
            }

            // The horizon binds the whole period, not just its start. Checking
            // only dateFrom let a request begin inside twelve months and end
            // years later, which is exactly what the rule forbids.
            var horizon = today.AddMonths(VacationCalculator.MaxMonthsAhead);
            if (dateTo > horizon)
            {
                throw Fail(
                    ErrorCodes.VacationTooFarAhead,
                    "tooFarAhead",
                    Resx.GetResourceString("tooFarAhead", VacationCalculator.MaxMonthsAhead),
                    new Dictionary<string, object> { ["months"] = VacationCalculator.MaxMonthsAhead });
            }

            // Before the overlap check, which would silently pass a zero-day period.
            if (VacationCalculator.CountWorkingDays(dateFrom, dateTo) == 0)
            {
                throw Fail(ErrorCodes.VacationNoWorkingDays, "noWorkingDays", Resx.GetResourceString("noWorkingDays"));
            }

            var clash = ownRequests.FirstOrDefault(request =>
                VacationCalculator.IsActive(request.Status) &&
                VacationCalculator.RangesOverlap(dateFrom, dateTo, request.DateFrom, request.DateTo));

            if (clash != null)
            {
                throw Fail(
                    ErrorCodes.VacationOverlap,
                    "overlap",
                    Resx.GetResourceString("vacationAlreadyExsists"),
                    new Dictionary<string, object>
                    {
                        ["from"] = VacationWireFormat.ToDay(clash.DateFrom),
                        ["to"] = VacationWireFormat.ToDay(clash.DateTo)
                    });
            }

            if ((note ?? string.Empty).Length > VacationRequest.MaxNoteLength)
            {
                throw Fail(
                    ErrorCodes.VacationNoteTooLong,
                    "noteTooLong",
                    Resx.GetResourceString("noteTooLong", VacationRequest.MaxNoteLength),
                    new Dictionary<string, object> { ["max"] = VacationRequest.MaxNoteLength });
            }

            // Deliberately no balance check: going over is a warning on the
            // approver's row, not a refusal.
        }

        /// <summary>
        /// Everything except a withdrawn request and anything that ended before
        /// the current month. A rejection stays editable within the open month —
        /// re-dating is how an employee answers one — and an approval stays
        /// editable even once the leave has started, so it can be cut short. Any
        /// edit returns the request to Pending for re-approval.
        /// </summary>
        public static bool CanEdit(VacationRequest request, DateTime today)
        {
            return request.Status != VacationRequestStatus.Cancelled
                   && !IsMonthClosed(request, today);
        }

        public static bool CanCancel(VacationRequest request, DateTime today)
        {
            return !IsMonthClosed(request, today)
                   && (request.Status == VacationRequestStatus.Pending
                       || (request.Status == VacationRequestStatus.Approved && request.DateFrom.Date > today));
        }

        /// <summary>
        /// Leave that finished before the first of the current month is closed:
        /// payroll has been run against it, so the employee cannot change it.
        /// Administrators are exempt — AdminEditAsync does not come through here.
        /// </summary>
        public static bool IsMonthClosed(VacationRequest request, DateTime today)
        {
            return request.DateTo.Date < new DateTime(today.Year, today.Month, 1);
        }

        public static void EnsureEditable(VacationRequest request, DateTime today)
        {
            if (IsMonthClosed(request, today))
            {
                throw Fail(ErrorCodes.VacationNotEditable, "monthClosed", Resx.GetResourceString("vacationMonthClosed"));
            }

            if (!CanEdit(request, today))
            {
                throw Fail(ErrorCodes.VacationNotEditable, "notEditable", Resx.GetResourceString("cannotEditVacation"));
            }
        }

        public static void EnsureCancellable(VacationRequest request, DateTime today)
        {
            if (IsMonthClosed(request, today))
            {
                throw Fail(ErrorCodes.VacationNotCancellable, "monthClosed", Resx.GetResourceString("vacationMonthClosed"));
            }

            if (!CanCancel(request, today))
            {
                throw Fail(ErrorCodes.VacationNotCancellable, "notCancellable", Resx.GetResourceString("vacationInProgress"));
            }
        }

        public static void EnsureReviewable(VacationRequest request)
        {
            if (request.Status != VacationRequestStatus.Pending)
            {
                throw Fail(ErrorCodes.VacationNotReviewable, "notReviewable", Resx.GetResourceString("cannotReviewVacation"));
            }
        }

        public static string ValidateRejectReason(string reason)
        {
            var trimmed = (reason ?? string.Empty).Trim();
            if (trimmed.Length < MinRejectReasonLength)
            {
                throw Fail(ErrorCodes.VacationReasonTooShort, "reasonTooShort", Resx.GetResourceString("reasonTooShort"));
            }

            return trimmed.Length > VacationRequest.MaxReviewCommentLength
                ? trimmed.Substring(0, VacationRequest.MaxReviewCommentLength)
                : trimmed;
        }

        public static VacationValidationException NotFound()
        {
            return Fail(ErrorCodes.VacationNotFound, "notFound", Resx.GetResourceString("noVacationFound"));
        }

        public static VacationValidationException NotAuthorized()
        {
            return Fail(ErrorCodes.VacationNotAuthorized, "notAuthorized", Resx.GetResourceString("notAuthorized"));
        }

        public static VacationValidationException Fail(
            int errorCode,
            string code,
            string message,
            IDictionary<string, object> parameters = null)
        {
            return new VacationValidationException(errorCode, code, message, parameters);
        }

        public static (VacationRequestType Type, DateTime DateFrom, DateTime DateTo, string Note) ParseDraft(
            string type,
            string dateFrom,
            string dateTo,
            string note)
        {
            var parsedType = VacationWireFormat.ParseType(type);
            if (parsedType == null)
            {
                throw Fail(ErrorCodes.VacationTypeRequired, "typeRequired", Resx.GetResourceString("typeNotSelected"));
            }

            var from = VacationWireFormat.ParseDay(dateFrom);
            if (from == null)
            {
                throw Fail(ErrorCodes.VacationDatesRequired, "datesRequired", Resx.GetResourceString("notEnoughData"));
            }

            var to = VacationWireFormat.ParseDay(dateTo);
            if (to == null && !VacationCalculator.IsSingleDayType(parsedType.Value))
            {
                throw Fail(ErrorCodes.VacationDatesRequired, "datesRequired", Resx.GetResourceString("notEnoughData"));
            }

            var effectiveTo = EffectiveDateTo(parsedType.Value, from.Value, to);
            var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

            return (parsedType.Value, from.Value, effectiveTo, trimmedNote);
        }
    }
}
