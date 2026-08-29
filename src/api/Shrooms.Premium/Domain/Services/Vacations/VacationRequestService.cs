using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Email.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationRequestService : IVacationRequestService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IHolidayService _holidayService;
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<VacationRequestEvent> _eventDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        private readonly IVacationNotificationService _notificationService;
        private readonly ILogger<VacationRequestService> _logger;

        public VacationRequestService(
            IUnitOfWork2 uow,
            IHolidayService holidayService,
            IVacationNotificationService notificationService,
            ILogger<VacationRequestService> logger)
        {
            _uow = uow;
            _holidayService = holidayService;
            _notificationService = notificationService;
            _logger = logger;
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _eventDbSet = uow.GetDbSet<VacationRequestEvent>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<VacationBalanceDto> GetBalanceAsync(UserAndOrganizationDto userOrg)
        {
            var user = await _userDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userOrg.UserId);

            if (user == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            var today = VacationCalculator.TodayIn(organization?.TimeZone);

            var entitlement = user.VacationUnusedTime ?? 0;
            var balanceAsOf = user.VacationLastTimeUpdated?.Date;

            // Pulled into memory: the cutoff-straddling case needs a working-day
            // count per row.
            var chargeable = await _requestDbSet
                .AsNoTracking()
                .Where(request => request.OrganizationId == userOrg.OrganizationId
                                  && request.EmployeeId == userOrg.UserId
                                  && request.Type == VacationRequestType.Annual
                                  && (request.Status == VacationRequestStatus.Pending
                                      || request.Status == VacationRequestStatus.Approved))
                .ToListAsync();

            var holidays = await _holidayService.GetCalendarAsync();
            var booked = VacationCalculator.CommittedAnnualDays(chargeable, balanceAsOf, holidays);
            var annualRate = VacationCalculator.AnnualAccrual(user.YearsEmployed);
            var accruedNow = VacationCalculator.ApproxAccruedNow(entitlement, balanceAsOf, today, annualRate);

            return new VacationBalanceDto
            {
                Entitlement = entitlement,
                BalanceAsOf = VacationWireFormat.ToDay(balanceAsOf),
                Booked = booked,
                Remaining = accruedNow - booked,
                AccruedNow = accruedNow,
                MonthlyAccrualRate = Math.Round(annualRate / 12, 2),
                YearsOfService = user.YearsEmployed
            };
        }

        public async Task<VacationRequestDto> SubmitAsync(VacationRequestDraftDto draft, UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);
            var parsed = VacationRequestValidator.ParseDraft(draft.Type, draft.DateFrom, draft.DateTo, draft.Note);

            var ownRequests = await OwnActiveRequestsAsync(userOrg.OrganizationId, userOrg.UserId, null);
            var holidays = await _holidayService.GetCalendarAsync();

            VacationRequestValidator.ValidateDraft(
                parsed.Type,
                parsed.DateFrom,
                parsed.DateTo,
                parsed.Note,
                today,
                ownRequests,
                holidays);

            var now = DateTime.UtcNow;
            var request = new VacationRequest
            {
                OrganizationId = userOrg.OrganizationId,
                EmployeeId = userOrg.UserId,
                Type = parsed.Type,
                Status = VacationRequestStatus.Pending,
                DateFrom = parsed.DateFrom,
                DateTo = parsed.DateTo,
                // Never the client's figure: it decides what the leave costs.
                WorkingDays = VacationCalculator.CountWorkingDays(parsed.DateFrom, parsed.DateTo, holidays),
                Note = parsed.Note,
                Created = now,
                Modified = now
            };

            _requestDbSet.Add(request);

            // One save for both rows: saving the request first for its id would
            // leave it with no "submitted" entry if the second save failed.
            AddEvent(request, VacationEventKind.Submitted, userOrg.UserId, request.Note);
            await _uow.SaveChangesAsync(userOrg.UserId);

            var submitted = await LoadDtoAsync(request.Id, userOrg.OrganizationId, today);
            await NotifyAsync(() => _notificationService.NotifySubmittedAsync(submitted, userOrg));

            return submitted;
        }

        public async Task<VacationRequestDto> EditAsync(int id, VacationRequestDraftDto draft, UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);
            var request = await FindOwnAsync(id, userOrg);

            VacationRequestValidator.EnsureEditable(request, today);

            var parsed = VacationRequestValidator.ParseDraft(draft.Type, draft.DateFrom, draft.DateTo, draft.Note);

            var ownRequests = await OwnActiveRequestsAsync(userOrg.OrganizationId, userOrg.UserId, id);
            var holidays = await _holidayService.GetCalendarAsync();

            VacationRequestValidator.ValidateDraft(
                parsed.Type,
                parsed.DateFrom,
                parsed.DateTo,
                parsed.Note,
                today,
                ownRequests,
                holidays,
                request.DateFrom);

            var before = Snapshot(request);

            request.Type = parsed.Type;
            request.DateFrom = parsed.DateFrom;
            request.DateTo = parsed.DateTo;
            request.WorkingDays = VacationCalculator.CountWorkingDays(parsed.DateFrom, parsed.DateTo, holidays);
            request.Note = parsed.Note;

            // Back to pending: the approved period is not the one being asked for
            // any more. The earlier decision survives in the log, which is why
            // clearing these is safe here and was not when cancelling.
            request.Status = VacationRequestStatus.Pending;
            request.ReviewedAt = null;
            request.ReviewedById = null;
            request.ReviewComment = null;

            var changes = VacationMapper.Diff(before, request);
            if (changes.Count == 0)
            {
                return await LoadDtoAsync(id, userOrg.OrganizationId, today);
            }

            AddEvent(request, VacationEventKind.Edited, userOrg.UserId, changes: changes);
            await _uow.SaveChangesAsync(userOrg.UserId);

            var edited = await LoadDtoAsync(id, userOrg.OrganizationId, today);
            await NotifyAsync(() => _notificationService.NotifyChangedAsync(edited, userOrg));

            return edited;
        }

        public async Task<VacationRequestDto> CancelAsync(int id, UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);
            var request = await FindOwnAsync(id, userOrg);

            VacationRequestValidator.EnsureCancellable(request, today);

            // Only the status changes: nulling the review trail would destroy the
            // record of who approved it.
            request.Status = VacationRequestStatus.Cancelled;

            AddEvent(request, VacationEventKind.Cancelled, userOrg.UserId);
            await _uow.SaveChangesAsync(userOrg.UserId);

            var cancelled = await LoadDtoAsync(id, userOrg.OrganizationId, today);
            await NotifyAsync(() => _notificationService.NotifyWithdrawnAsync(cancelled, userOrg));

            return cancelled;
        }

        public async Task<VacationRequestDto> GetForReviewAsync(int id, UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);

            var request = await _requestDbSet
                .AsNoTracking()
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == userOrg.OrganizationId);

            if (request == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            EnsureReviewer(request, userOrg);

            return await LoadDtoAsync(id, userOrg.OrganizationId, today);
        }

        public Task<VacationRequestDto> ApproveAsync(int id, UserAndOrganizationDto userOrg)
        {
            return ReviewAsync(id, VacationRequestStatus.Approved, null, userOrg);
        }

        public Task<VacationRequestDto> RejectAsync(int id, string reason, UserAndOrganizationDto userOrg)
        {
            var comment = VacationRequestValidator.ValidateRejectReason(reason);
            return ReviewAsync(id, VacationRequestStatus.Rejected, comment, userOrg);
        }

        public async Task<VacationRequestDto> AdminEditAsync(int id, VacationAdminPatchDto patch, UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);

            var request = await _requestDbSet
                .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == userOrg.OrganizationId);

            if (request == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            var type = VacationWireFormat.ParseType(patch.Type) ?? request.Type;
            var status = VacationWireFormat.ParseStatus(patch.Status) ?? request.Status;
            var dateFrom = VacationWireFormat.ParseDay(patch.DateFrom) ?? request.DateFrom;
            var dateTo = VacationWireFormat.ParseDay(patch.DateTo) ?? request.DateTo;

            // No date rules here: an administrator is correcting a record, not
            // asking for leave.
            if (VacationCalculator.IsSingleDayType(type))
            {
                dateTo = dateFrom;
            }

            if (dateTo < dateFrom)
            {
                (dateFrom, dateTo) = (dateTo, dateFrom);
            }

            var before = Snapshot(request);

            request.Type = type;
            request.Status = status;
            request.DateFrom = dateFrom;
            request.DateTo = dateTo;
            request.WorkingDays = VacationCalculator.CountWorkingDays(dateFrom, dateTo, await _holidayService.GetCalendarAsync());
            // Null means "leave it", as with the fields above; the dialog sends
            // an empty string to clear one.
            if (patch.Note != null)
            {
                request.Note = Truncate(patch.Note, VacationRequest.MaxNoteLength);
            }

            // A decision names who made it, a request awaiting one has none, and a
            // withdrawal keeps whatever preceded it.
            switch (status)
            {
                case VacationRequestStatus.Pending:
                    request.ReviewedAt = null;
                    request.ReviewedById = null;
                    request.ReviewComment = null;
                    break;
                case VacationRequestStatus.Approved:
                case VacationRequestStatus.Rejected:
                    request.ReviewedAt = DateTime.UtcNow;
                    request.ReviewedById = userOrg.UserId;
                    break;
            }

            var changes = VacationMapper.Diff(before, request);
            if (changes.Count == 0)
            {
                return await LoadDtoAsync(id, userOrg.OrganizationId, today);
            }

            AddEvent(request, VacationEventKind.Edited, userOrg.UserId, changes: changes);
            await _uow.SaveChangesAsync(userOrg.UserId);

            var patched = await LoadDtoAsync(id, userOrg.OrganizationId, today);

            // A status the administrator set is a decision the manager was
            // waiting to make; anything else is a correction only the employee
            // needs to hear about.
            var decided = before.Status != request.Status;
            await NotifyAsync(() => decided
                ? _notificationService.NotifyDecidedAsync(patched, userOrg)
                : _notificationService.NotifyChangedAsync(patched, userOrg));

            return patched;
        }

        /// <summary>
        /// A change is saved before anybody is told about it, and a mail server
        /// that will not take the message must not fail the change that already
        /// happened.
        /// </summary>
        private async Task NotifyAsync(Func<Task> send)
        {
            try
            {
                await send();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Vacation notification failed");
            }
        }

        private async Task<VacationRequestDto> ReviewAsync(
            int id,
            VacationRequestStatus status,
            string comment,
            UserAndOrganizationDto userOrg)
        {
            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);

            var request = await _requestDbSet
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == userOrg.OrganizationId);

            if (request == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            EnsureReviewer(request, userOrg);

            VacationRequestValidator.EnsureReviewable(request);

            request.Status = status;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedById = userOrg.UserId;
            request.ReviewComment = comment;

            AddEvent(
                request,
                status == VacationRequestStatus.Approved ? VacationEventKind.Approved : VacationEventKind.Rejected,
                userOrg.UserId,
                comment);

            await _uow.SaveChangesAsync(userOrg.UserId);

            var reviewed = await LoadDtoAsync(id, userOrg.OrganizationId, today);
            await NotifyAsync(() => _notificationService.NotifyDecidedAsync(reviewed, userOrg));

            return reviewed;
        }

        private static void EnsureReviewer(VacationRequest request, UserAndOrganizationDto userOrg)
        {
            // Only the employee's own manager reviews. No administrator fallback:
            // a request with no manager stays pending until HR sets one.
            if (request.Employee?.ManagerId != userOrg.UserId)
            {
                throw VacationRequestValidator.NotAuthorized();
            }
        }

        private void AddEvent(
            VacationRequest request,
            VacationEventKind kind,
            string actorId,
            string comment = null,
            IList<VacationFieldChangeDto> changes = null)
        {
            var now = DateTime.UtcNow;

            _eventDbSet.Add(new VacationRequestEvent
            {
                OrganizationId = request.OrganizationId,
                // The navigation rather than the id: on a brand-new request the
                // id is still 0 until SaveChanges runs, and EF resolves the key
                // from the reference.
                VacationRequest = request,
                Kind = kind,
                ActorId = actorId,
                OccurredAt = now,
                EmployeeId = request.EmployeeId,
                Type = request.Type,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                WorkingDays = request.WorkingDays,
                Comment = Truncate(comment, VacationRequestEvent.MaxCommentLength),
                ChangesJson = VacationMapper.SerializeChanges(changes),
                Created = now,
                Modified = now
            });
        }

        private async Task<VacationRequest> FindOwnAsync(int id, UserAndOrganizationDto userOrg)
        {
            var request = await _requestDbSet
                .FirstOrDefaultAsync(r => r.Id == id
                                          && r.OrganizationId == userOrg.OrganizationId
                                          && r.EmployeeId == userOrg.UserId);

            if (request == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            return request;
        }

        private async Task<List<VacationRequest>> OwnActiveRequestsAsync(int organizationId, string userId, int? excludeId)
        {
            return await _requestDbSet
                .AsNoTracking()
                .Where(request => request.OrganizationId == organizationId
                                  && request.EmployeeId == userId
                                  && request.Id != excludeId
                                  && (request.Status == VacationRequestStatus.Pending
                                      || request.Status == VacationRequestStatus.Approved))
                .ToListAsync();
        }

        private async Task<VacationRequestDto> LoadDtoAsync(int id, int organizationId, DateTime today)
        {
            var request = await _requestDbSet
                .AsNoTracking()
                .Include(r => r.Employee)
                .Include(r => r.ReviewedBy)
                .FirstAsync(r => r.Id == id && r.OrganizationId == organizationId);

            return VacationMapper.ToRequest(request, today);
        }

        /// <summary>EF tracks the same instance either side of a mutation.</summary>
        private static VacationRequest Snapshot(VacationRequest request)
        {
            return new VacationRequest
            {
                Type = request.Type,
                Status = request.Status,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                Note = request.Note
            };
        }

        private static string Truncate(string value, int max)
        {
            var trimmed = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (trimmed == null)
            {
                return null;
            }

            return trimmed.Length > max ? trimmed.Substring(0, max) : trimmed;
        }
    }
}
