using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.Domain.Services.Organizations;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.Domain.Services.Email.Event
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

        public void NotifyRemovedEventParticipants(string eventName, Guid eventId, int orgId, IEnumerable<string> users)
        {
            var organization = _organizationService.GetOrganizationById(orgId);
            var emails = _usersDbSet
                .Where(u => users.Contains(u.Id))
                .Select(u => u.Email)
                .ToList();

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, eventId.ToString());

            var emailTemplateViewModel = new EventParticipantExpelledEmailTemplateViewModel(userNotificationSettingsUrl, eventName, eventUrl);
            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.EventParticipantExpelled);

            _mailingService.SendEmail(new EmailDto(emails, Resources.Models.Events.Events.ResetParticipantListEmailSubject, emailBody));
        }

        public void RemindUsersToJoinEvent(EventTypeDTO eventType, IEnumerable<string> emails, int orgId)
        {
            var organization = _organizationService.GetOrganizationById(orgId);

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var eventUrl = _appSettings.EventListByTypeUrl(organization.ShortName, eventType.Id.ToString());

            var emailTemplateViewModel = new EventJoinRemindEmailTemplateViewModel(userNotificationSettingsUrl)
            {
                EventPageUrl = eventUrl,
                EventTypeName = eventType.Name
            };
            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.EventJoinRemind);

            _mailingService.SendEmail(new EmailDto(emails, $"Join {eventType.Name} event now", emailBody));
        }
    }
}
