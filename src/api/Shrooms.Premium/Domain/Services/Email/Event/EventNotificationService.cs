using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Email.Event
{
    public class EventNotificationService : IEventNotificationService
    {
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;

        private readonly IDbSet<ApplicationUser> _usersDbSet;

        public EventNotificationService(
            IUnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;

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
        
        public async Task RemindAllUsersAboutJoinedEventsAsync(RemindJoinedEventEmailDto remindDto, Organization organization)
        {
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var emailsToSend = remindDto.RemindStartEvents.Select(MapEventStartEmailsToEmailContent(organization, userNotificationSettingsUrl))
                .Union(remindDto.RemindDeadlineEvents.Select(MapEventDeadlineEmailsToEmailContent(organization, userNotificationSettingsUrl)))
                .ToList();
            
            foreach (var emailContent in emailsToSend)
            {
                await _mailingService.SendEmailAsync(new EmailDto(emailContent.UserEmails, emailContent.Subject, emailContent.Body));
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

        private Func<RemindEventStartEmailDto, (string Body, IEnumerable<string> UserEmails, string Subject)> MapEventStartEmailsToEmailContent(Organization organization, string userNotificationSettingsUrl)
        {
            return reminder =>
            {
                var eventUrl = _appSettings.EventUrl(organization.ShortName, reminder.EventId.ToString());
                var eventEmailTemplate = new EventStartRemindEmailTemplateViewModel(
                    userNotificationSettingsUrl,
                    reminder.EventName,
                    eventUrl,
                    reminder.StartDate);
                var emailBody = _mailTemplate.Generate(eventEmailTemplate, EmailPremiumTemplateCacheKeys.EventStartRemind);
                var subject = string.Format(Resources.Models.Events.Events.RemindEventStartEmailSubject, reminder.EventName);
                return (Body: emailBody, reminder.UserEmails, Subject: subject);
            };
        }

        private Func<RemindEventDeadlineEmailDto, (string Body, IEnumerable<string> UserEmails, string Subject)> MapEventDeadlineEmailsToEmailContent(Organization organization, string userNotificationSettingsUrl)
        {
            return reminder =>
            {
                var eventUrl = _appSettings.EventUrl(organization.ShortName, reminder.EventId.ToString());
                var eventEmailTemplate = new EventDeadlineRemindEmailTemplateViewModel(
                    userNotificationSettingsUrl,
                    reminder.EventName,
                    eventUrl,
                    reminder.StartDate,
                    reminder.DeadlineDate);
                var emailBody = _mailTemplate.Generate(eventEmailTemplate, EmailPremiumTemplateCacheKeys.EventDeadlineRemind);
                var subject = string.Format(Resources.Models.Events.Events.RemindEventDeadlineEmailSubject, reminder.EventName);
                return (Body: emailBody, reminder.UserEmails, Subject: subject);
            };
        }
    }
}
