using System.Globalization;
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
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Domain.Services.Email.TimeRegistration
{
    public interface ITimeOvertimeNotificationService
    {
        Task NotifyAsync(TimeOvertimeRequest request, UserAndOrganizationDto actor);
    }

    public class TimeOvertimeNotificationService : NotificationServiceBase, ITimeOvertimeNotificationService
    {
        private const string OwnTitle = "Overtime";

        private readonly IApplicationSettings _appSettings;
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<ApplicationUser> _users;
        private readonly DbSet<Organization> _organizations;
        private readonly DbSet<Notification> _notifications;

        public TimeOvertimeNotificationService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate)
            : base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;
            _uow = uow;
            _users = uow.GetDbSet<ApplicationUser>();
            _organizations = uow.GetDbSet<Organization>();
            _notifications = uow.GetDbSet<Notification>();
        }

        public async Task NotifyAsync(TimeOvertimeRequest request, UserAndOrganizationDto actor)
        {
            var employee = await PersonAsync(request.UserId);
            if (employee == null)
            {
                return;
            }

            var recipientId = TimeRegistrationRules.SameUser(actor.UserId, employee.Id) ? employee.ManagerId : employee.Id;
            if (string.IsNullOrEmpty(recipientId) || TimeRegistrationRules.SameUser(recipientId, actor.UserId))
            {
                return;
            }

            var recipient = await PersonAsync(recipientId);
            if (recipient == null)
            {
                return;
            }

            var organization = await _organizations
                .AsNoTracking()
                .Where(row => row.Id == actor.OrganizationId)
                .Select(row => row.ShortName)
                .FirstOrDefaultAsync();
            var actorName = (await PersonAsync(actor.UserId))?.FullName ?? string.Empty;
            var toEmployee = TimeRegistrationRules.SameUser(recipient.Id, employee.Id);

            if (!string.IsNullOrEmpty(recipient.Email))
            {
                await SendEmailAsync(request, employee.FullName, actorName, organization, recipient.Email, toEmployee);
            }

            if (recipient.WantsApp)
            {
                _notifications.Add(Notification.Create(
                    toEmployee ? OwnTitle : employee.FullName,
                    $"{Period(request)} · {Hours(request.MinutesPerDay)} h a day — {Outcome(request)}",
                    null,
                    new Sources(),
                    toEmployee ? NotificationType.OvertimeRequest : NotificationType.OvertimeReview,
                    actor.OrganizationId,
                    new[] { recipient.Id }));
                await _uow.SaveChangesAsync(actor.UserId);
            }
        }

        private async Task SendEmailAsync(
            TimeOvertimeRequest request,
            string employeeName,
            string actorName,
            string organization,
            string email,
            bool toEmployee)
        {
            var settingsUrl = GetNotificationSettingsUrl(organization);
            var period = Period(request);
            var hours = Hours(request.MinutesPerDay);

            if (request.Status == TimeOvertimeRequestStatus.Pending)
            {
                var model = new OvertimeDecidableEmailTemplateViewModel(
                    settingsUrl,
                    employeeName,
                    period,
                    hours,
                    request.Reason,
                    toEmployee ? _appSettings.OvertimeRequestsUrl(organization) : _appSettings.OvertimeQueueUrl(organization),
                    _appSettings.OvertimeApproveUrl(organization, request.Id),
                    _appSettings.OvertimeRejectUrl(organization, request.Id))
                {
                    ActorName = actorName
                };

                if (request.OrderedByManager)
                {
                    await SendSingleEmailAsync(email, $"{actorName} asks you to work overtime", model, EmailPremiumTemplateCacheKeys.OvertimeOrdered);
                }
                else
                {
                    await SendSingleEmailAsync(email, $"{employeeName} asks to work overtime", model, EmailPremiumTemplateCacheKeys.OvertimeRequested);
                }

                return;
            }

            var decided = new OvertimeEmailTemplateViewModel(
                settingsUrl,
                employeeName,
                period,
                hours,
                request.Reason,
                toEmployee ? _appSettings.OvertimeRequestsUrl(organization) : _appSettings.OvertimeQueueUrl(organization))
            {
                ActorName = actorName,
                Outcome = Outcome(request),
                Note = request.ReviewComment
            };
            await SendSingleEmailAsync(email, $"Overtime {Outcome(request)}: {period}", decided, EmailPremiumTemplateCacheKeys.OvertimeDecided);
        }

        private async Task<Person> PersonAsync(string userId)
        {
            var row = await _users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(user => user.Id == userId)
                .Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ManagerId,
                    WantsApp = user.NotificationsSettings == null || user.NotificationsSettings.OvertimeAppNotifications
                })
                .FirstOrDefaultAsync();

            return row == null
                ? null
                : new Person(row.Id, TimeTableService.FullName(row.FirstName, row.LastName), row.Email, row.ManagerId, row.WantsApp);
        }

        private static string Period(TimeOvertimeRequest request)
        {
            return request.DateFrom == request.DateTo
                ? TimeMonth.ToDay(request.DateFrom)
                : $"{TimeMonth.ToDay(request.DateFrom)} – {TimeMonth.ToDay(request.DateTo)}";
        }

        private static string Hours(int minutes)
        {
            return (minutes / 60m).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string Outcome(TimeOvertimeRequest request)
        {
            return request.Status switch
            {
                TimeOvertimeRequestStatus.Pending => request.OrderedByManager ? "waiting for your consent" : "waiting for approval",
                TimeOvertimeRequestStatus.Approved => request.OrderedByManager ? "agreed" : "approved",
                TimeOvertimeRequestStatus.Rejected => request.OrderedByManager ? "declined" : "rejected",
                _ => "cancelled"
            };
        }

        private sealed record Person(string Id, string FullName, string Email, string ManagerId, bool WantsApp);
    }
}
