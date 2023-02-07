using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Events;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Email.Converters;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Email.Event
{
    public class EventNotificationService : IEventNotificationService
    {
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IMarkdownConverter _markdownConverter;
        private readonly IMailTemplateConverter _mailTemplateConverter;

        private readonly IDbSet<ApplicationUser> _usersDbSet;

        public EventNotificationService(
            IUnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService,
            IMarkdownConverter markdownConverter,
            IMailTemplateConverter mailTemplateConverter)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;
            _mailTemplateConverter = mailTemplateConverter;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task NotifyRemovedEventParticipantsAsync(string eventName, Guid eventId, int orgId, IEnumerable<string> users)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(orgId);
            var emails = await _usersDbSet
                .Where(u => users.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, eventId.ToString());

            var emailTemplateViewModel = new EventParticipantExpelledEmailTemplateViewModel(userNotificationSettingsUrl, eventName, eventUrl);
            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.EventParticipantExpelled);

            await _mailingService.SendEmailAsync(new EmailDto(emails, Resources.Models.Events.Events.ResetParticipantListEmailSubject, emailBody));
        }
        
        public async Task RemindUsersAboutDeadlineDateOfJoinedEventsAsync(IEnumerable<EventReminderDeadlineEmailDto> deadlineEmailDtos, Organization organization)
        {
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            foreach (var deadlineEmailDto in deadlineEmailDtos)
            {
                await RemindUsersAboutDeadlineDateOfJoinedEventAsync(deadlineEmailDto, organization, userNotificationSettingsUrl);
            }
        }

        public async Task RemindUsersAboutStartDateOfJoinedEventsAsync(IEnumerable<EventReminderStartEmailDto> startEmailDtos, Organization organization)
        {
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            foreach (var startEmailDto in startEmailDtos)
            {
                await RemindUsersAboutStartDateOfJoinedEventAsync(startEmailDto, organization, userNotificationSettingsUrl);
            }
        }

        public async Task RemindUsersToJoinEventAsync(IEnumerable<EventTypeDto> eventTypes, IEnumerable<string> emails, int orgId)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(orgId);

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var emailTemplateViewModel = new EventJoinRemindEmailTemplateViewModel(userNotificationSettingsUrl);

            foreach (var eventType in eventTypes)
            {
                emailTemplateViewModel.EventTypes.Add(eventType.Name, _appSettings.EventListByTypeUrl(organization.ShortName, eventType.Id.ToString()));
            }

            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.EventJoinRemind);
            await _mailingService.SendEmailAsync(new EmailDto(emails, $"Join weekly event now", emailBody));
        }

        public async Task NotifyManagerAboutEventAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto, bool isJoiningEvent)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(userAttendStatusDto.OrganizationId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, userAttendStatusDto.EventId.ToString());

            var emailDto = GetManagerNotifyEmailDto(userAttendStatusDto, userNotificationSettingsUrl, eventUrl, isJoiningEvent);

            await _mailingService.SendEmailAsync(emailDto);
        }

        public async Task NotifySharedEventAsync(SharedEventEmailDto shareEventEmailDto, UserAndOrganizationHubDto userOrgHubDto)
        {
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(userOrgHubDto.OrganizationName);
            var postUrl = _appSettings.WallPostUrl(userOrgHubDto.OrganizationName, shareEventEmailDto.CreatedPost.Id);
            var subject = string.Format(
                Resources.Models.Events.Events.ShareEventEmailSubject,
                shareEventEmailDto.Details.Name,
                shareEventEmailDto.CreatedPost.WallName);
            var body = _markdownConverter.ConvertToHtml(shareEventEmailDto.CreatedPost.MessageBody);

            var emailTemplate = new SharedEventEmailTemplateViewModel(
                postUrl,
                shareEventEmailDto.CreatedPost.User.FullName,
                shareEventEmailDto.CreatedPost.MessageBody,
                shareEventEmailDto.CreatedPost.WallName,
                shareEventEmailDto.Details.Name,
                shareEventEmailDto.Details.StartDate,
                shareEventEmailDto.Details.EndDate,
                shareEventEmailDto.Details.RegistrationDeadlineDate,
                shareEventEmailDto.Details.TypeName,
                shareEventEmailDto.Details.Description,
                userNotificationSettingsUrl);
            var compiledEmailBodiesByTimeZone = await _mailTemplateConverter.ConvertEmailTemplateToReceiversTimeZoneSettingsAsync(
                emailTemplate,
                EmailPremiumTemplateCacheKeys.EventShared,
                shareEventEmailDto.Receivers,
                template => template.StartDate,
                template => template.EndDate,
                template => template.RegistrationDeadlineDate);
            await SendSameEmailWithDifferentTimeZonesAsync(compiledEmailBodiesByTimeZone, subject);
        }

        private EmailDto GetManagerNotifyEmailDto(UserEventAttendStatusChangeEmailDto userAttendStatusDto, string userNotificationSettingsUrl, string eventUrl, bool isJoiningEvent)
        {
            if (!isJoiningEvent)
            {
                var emailTemplateLeaveViewModel = new CoacheeLeftEventEmailTemplateViewModel(
                    userNotificationSettingsUrl,
                    userAttendStatusDto,
                    eventUrl);

                var emailLeaveBody = _mailTemplate.Generate(emailTemplateLeaveViewModel, EmailPremiumTemplateCacheKeys.CoacheeLeftEvent);

                var emailLeaveSubject = string.Format(Resources.Models.Events.Events.CoacheeLeftEventEmailSubject,
                    userAttendStatusDto.FullName, userAttendStatusDto.EventName);

                return new EmailDto(new List<string> { userAttendStatusDto.ManagerEmail },
                    emailLeaveSubject,
                    emailLeaveBody);
            }

            var emailTemplateJoinViewModel = new CoacheeJoinedEventEmailTemplateViewModel(
                userNotificationSettingsUrl,
                userAttendStatusDto,
                eventUrl);

            var emailJoinBody = _mailTemplate.Generate(emailTemplateJoinViewModel, EmailPremiumTemplateCacheKeys.CoacheeJoinedEvent);

            var emailJoinSubject = string.Format(Resources.Models.Events.Events.CoacheeJoinedEventEmailSubject,
                userAttendStatusDto.FullName, userAttendStatusDto.EventName);

            return new EmailDto(new List<string> { userAttendStatusDto.ManagerEmail },
                    emailJoinSubject,
                    emailJoinBody);
        }

        private async Task SendSameEmailWithDifferentTimeZonesAsync(IEnumerable<CompiledEmailTemplateWithReceiverEmails> compiledTemplates, string subject)
        {
            foreach (var compiledTemplate in compiledTemplates)
            {
                await _mailingService.SendEmailAsync(new EmailDto(compiledTemplate.ReceiverEmails, subject, compiledTemplate.Body));
            }
        }

        private async Task RemindUsersAboutDeadlineDateOfJoinedEventAsync(EventReminderDeadlineEmailDto deadlineEmailDto, Organization organization, string userNotificationSettingsUrl)
        {
            var subject = string.Format(Resources.Models.Events.Events.RemindEventDeadlineEmailSubject, deadlineEmailDto.EventName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, deadlineEmailDto.EventId.ToString());
            var emailTemplate = new EventReminderDeadlineEmailTemplateViewModel(
                userNotificationSettingsUrl,
                deadlineEmailDto.EventName,
                eventUrl,
                deadlineEmailDto.StartDate,
                deadlineEmailDto.DeadlineDate);

            var compiledEmailBodiesByTimeZone = await _mailTemplateConverter.ConvertEmailTemplateToReceiversTimeZoneSettingsAsync(
                emailTemplate,
                EmailPremiumTemplateCacheKeys.EventDeadlineRemind,
                deadlineEmailDto.Receivers,
                template => template.StartDate,
                template => template.DeadlineDate);
            await SendSameEmailWithDifferentTimeZonesAsync(compiledEmailBodiesByTimeZone, subject);
        }

        private async Task RemindUsersAboutStartDateOfJoinedEventAsync(EventReminderStartEmailDto startEmailDto, Organization organization, string userNotificationSettingsUrl)
        {
            var subject = string.Format(Resources.Models.Events.Events.RemindEventStartEmailSubject, startEmailDto.EventName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, startEmailDto.EventId.ToString());
            var emailTemplate = new EventReminderStartEmailTemplateViewModel(
                userNotificationSettingsUrl,
                startEmailDto.EventName,
                eventUrl,
                startEmailDto.StartDate);

            var compiledEmailBodiesByTimeZone = await _mailTemplateConverter.ConvertEmailTemplateToReceiversTimeZoneSettingsAsync(
                emailTemplate,
                EmailPremiumTemplateCacheKeys.EventStartRemind,
                startEmailDto.Receivers,
                template => template.StartDate);
            await SendSameEmailWithDifferentTimeZonesAsync(compiledEmailBodiesByTimeZone, subject);
        }
    }
}