using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Email.Vacations
{
    public class VacationNotificationService : NotificationServiceBase, IVacationNotificationService
    {
        private const string PendingStatus = "pending";

        private const string OwnTitle = "Vacation";

        private readonly IApplicationSettings _appSettings;
        private readonly IUnitOfWork2 _uow;

        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;
        private readonly DbSet<Notification> _notificationDbSet;

        public VacationNotificationService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate)
            : base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;
            _uow = uow;

            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
            _notificationDbSet = uow.GetDbSet<Notification>();
        }

        public Task NotifySubmittedAsync(VacationRequestDto request, UserAndOrganizationDto actor)
        {
            return SendAsync(request, actor, (employeeId, managerId, actorId) =>
                VacationNotificationRecipients.ForSubmitted(managerId, actorId));
        }

        public Task NotifyChangedAsync(VacationRequestDto request, UserAndOrganizationDto actor)
        {
            return SendAsync(request, actor, VacationNotificationRecipients.ForChanged);
        }

        public Task NotifyWithdrawnAsync(VacationRequestDto request, UserAndOrganizationDto actor)
        {
            return SendAsync(request, actor, (employeeId, managerId, actorId) =>
                VacationNotificationRecipients.ForWithdrawn(managerId, actorId));
        }

        public Task NotifyDecidedAsync(VacationRequestDto request, UserAndOrganizationDto actor)
        {
            return SendAsync(request, actor, VacationNotificationRecipients.ForDecided);
        }

        private async Task SendAsync(
            VacationRequestDto request,
            UserAndOrganizationDto actor,
            Func<string, string, string, IList<VacationRecipient>> choose)
        {
            var employeeId = request.Employee?.Id;
            if (string.IsNullOrEmpty(employeeId))
            {
                return;
            }

            var managerId = await _userDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(user => user.Id == employeeId)
                .Select(user => user.ManagerId)
                .FirstOrDefaultAsync();

            var recipients = choose(employeeId, managerId, actor.UserId);
            if (recipients.Count == 0)
            {
                return;
            }

            var ids = recipients.Select(recipient => recipient.UserId).ToList();
            var people = await _userDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(user => ids.Contains(user.Id))
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    Wants = user.NotificationsSettings == null || user.NotificationsSettings.VacationsAppNotifications
                })
                .ToListAsync();

            var organizationName = await _organizationDbSet
                .AsNoTracking()
                .Where(organization => organization.Id == actor.OrganizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();

            var actorName = await FullNameAsync(actor.UserId);
            var settingsUrl = GetNotificationSettingsUrl(organizationName);
            var inApp = new List<string>();

            foreach (var recipient in recipients)
            {
                var person = people.FirstOrDefault(candidate => candidate.Id == recipient.UserId);
                if (person == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(person.Email))
                {
                    await SendOneAsync(request, recipient, organizationName, settingsUrl, actorName, person.Email);
                }

                if (person.Wants)
                {
                    inApp.Add(recipient.UserId);
                }
            }

            await AddInAppAsync(request, actor, inApp);
        }

        private async Task SendOneAsync(
            VacationRequestDto request,
            VacationRecipient recipient,
            string organizationName,
            string settingsUrl,
            string actorName,
            string email)
        {
            var period = Period(request);
            var days = request.WorkingDays.ToString("0.##");
            var leaveType = LeaveType(request.Type);

            switch (recipient.Notice)
            {
                case VacationNotice.Submitted:
                    await SendSingleEmailAsync(
                        email,
                        string.Format(Resx.GetResourceString("notificationSubmittedSubject"), request.Employee.FullName),
                        Decidable(request, settingsUrl, period, leaveType, days, actorName, organizationName, recipient),
                        EmailPremiumTemplateCacheKeys.VacationSubmitted);
                    return;

                case VacationNotice.Changed:
                    await SendSingleEmailAsync(
                        email,
                        string.Format(Resx.GetResourceString("notificationChangedSubject"), request.Employee.FullName),
                        Decidable(request, settingsUrl, period, leaveType, days, actorName, organizationName, recipient),
                        EmailPremiumTemplateCacheKeys.VacationChanged);
                    return;

                case VacationNotice.Withdrawn:
                    await SendSingleEmailAsync(
                        email,
                        string.Format(Resx.GetResourceString("notificationWithdrawnSubject"), request.Employee.FullName),
                        Model(request, settingsUrl, period, leaveType, days, actorName, _appSettings.VacationQueueUrl(organizationName)),
                        EmailPremiumTemplateCacheKeys.VacationWithdrawn);
                    return;

                case VacationNotice.Decided:
                    await SendSingleEmailAsync(
                        email,
                        string.Format(Resx.GetResourceString("notificationDecidedSubject"), Outcome(request.Status)),
                        Decided(request, settingsUrl, period, leaveType, days, actorName, _appSettings.VacationRequestsUrl(organizationName)),
                        EmailPremiumTemplateCacheKeys.VacationDecided);
                    return;

                case VacationNotice.DecidedByAdmin:
                    await SendSingleEmailAsync(
                        email,
                        string.Format(Resx.GetResourceString("notificationDecidedByAdminSubject"), request.Employee.FullName),
                        Decided(request, settingsUrl, period, leaveType, days, actorName, _appSettings.VacationQueueUrl(organizationName)),
                        EmailPremiumTemplateCacheKeys.VacationDecidedByAdmin);
                    return;
            }
        }

        /// <summary>
        /// One row per audience rather than one shared row. The employee's own
        /// leave is headed "Vacation" and opens their list; their manager needs to
        /// see whose leave it is, and lands on the queue. The notification type is
        /// what tells the client which of the two it is holding.
        /// </summary>
        private async Task AddInAppAsync(VacationRequestDto request, UserAndOrganizationDto actor, IList<string> userIds)
        {
            if (userIds.Count == 0)
            {
                return;
            }

            var own = userIds.Where(id => id == request.Employee.Id).ToList();
            var others = userIds.Where(id => id != request.Employee.Id).ToList();

            AddRow(request, actor, own, OwnTitle, NotificationType.VacationRequest);
            AddRow(request, actor, others, request.Employee.FullName, NotificationType.VacationReview);

            await _uow.SaveChangesAsync(actor.UserId);
        }

        private void AddRow(VacationRequestDto request, UserAndOrganizationDto actor, IList<string> userIds, string title, NotificationType type)
        {
            if (userIds.Count == 0)
            {
                return;
            }

            _notificationDbSet.Add(Notification.Create(
                title,
                $"{Period(request)} — {Outcome(request.Status)}",
                // No avatar: the client heads these with a leave icon, and the
                // employee's own face told them nothing.
                null,
                new Sources { VacationRequestId = request.Id },
                type,
                actor.OrganizationId,
                userIds));
        }

        /// <summary>
        /// A request that is still waiting for a decision. The Approve and Reject
        /// buttons go only to the person who can press them: the employee gets the
        /// same mail without them, and so does anybody whose copy describes a
        /// request that has already been settled.
        /// </summary>
        private VacationSubmittedEmailTemplateViewModel Decidable(
            VacationRequestDto request,
            string settingsUrl,
            string period,
            string leaveType,
            string days,
            string actorName,
            string organizationName,
            VacationRecipient recipient)
        {
            var reviewer = recipient.UserId != request.Employee.Id
                           && request.Status == PendingStatus;

            return new VacationSubmittedEmailTemplateViewModel(
                settingsUrl,
                request.Employee.FullName,
                period,
                leaveType,
                days,
                reviewer
                    ? _appSettings.VacationQueueUrl(organizationName)
                    : _appSettings.VacationRequestsUrl(organizationName),
                reviewer ? _appSettings.VacationApproveUrl(organizationName, request.Id) : null,
                reviewer ? _appSettings.VacationRejectUrl(organizationName, request.Id) : null)
            {
                Note = request.Note,
                ActorName = actorName
            };
        }

        private static VacationEmailTemplateViewModel Model(
            VacationRequestDto request,
            string settingsUrl,
            string period,
            string leaveType,
            string days,
            string actorName,
            string url)
        {
            return new VacationEmailTemplateViewModel(settingsUrl, request.Employee.FullName, period, leaveType, days, url)
            {
                Note = request.Note,
                ActorName = actorName
            };
        }

        /// <summary>A rejection carries its reason where an ordinary change carries the note.</summary>
        private static VacationEmailTemplateViewModel Decided(
            VacationRequestDto request,
            string settingsUrl,
            string period,
            string leaveType,
            string days,
            string actorName,
            string url)
        {
            var model = Model(request, settingsUrl, period, leaveType, days, actorName, url);
            model.Outcome = Outcome(request.Status);
            model.Note = string.IsNullOrWhiteSpace(request.ReviewComment) ? request.Note : request.ReviewComment;
            return model;
        }

        private async Task<string> FullNameAsync(string userId)
        {
            var name = await _userDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(user => user.Id == userId)
                .Select(user => new { user.FirstName, user.LastName })
                .FirstOrDefaultAsync();

            return name == null ? string.Empty : $"{name.FirstName} {name.LastName}".Trim();
        }

        private static string Period(VacationRequestDto request)
        {
            return request.DateFrom == request.DateTo
                ? request.DateFrom
                : $"{request.DateFrom} – {request.DateTo}";
        }

        /// <summary>
        /// Deliberately not from the resources. Every email template in the app
        /// is written in English, and pulling these two words from the sender's
        /// culture produced half-Lithuanian sentences inside an English mail.
        /// The in-app notification, which the app does localise, still uses the
        /// resource strings below.
        /// </summary>
        private static string LeaveType(string type)
        {
            return type switch
            {
                "annual" => "Annual leave",
                "parental" => "Parental day",
                "unpaid" => "Unpaid leave",
                _ => type
            };
        }

        private static string Outcome(string status)
        {
            return status switch
            {
                PendingStatus => "waiting for a decision",
                "approved" => "approved",
                "rejected" => "rejected",
                "cancelled" => "cancelled",
                _ => status
            };
        }

    }
}
