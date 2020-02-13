using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Domain.Services.Organizations;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.DataTransferObjects;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Host.Contracts.Infrastructure.Email;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Event
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

        public void RemindUsersToJoinEvent(IEnumerable<EventTypeDTO> eventTypes, IEnumerable<string> emails, int orgId)
        {
            var organization = _organizationService.GetOrganizationById(orgId);

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var emailTemplateViewModel = new EventJoinRemindEmailTemplateViewModel(userNotificationSettingsUrl);

            foreach (var eventType in eventTypes)
            {
                emailTemplateViewModel.EventTypes.Add(eventType.Name, _appSettings.EventListByTypeUrl(organization.ShortName, eventType.Id.ToString()));
            }

            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.EventJoinRemind);
            _mailingService.SendEmail(new EmailDto(emails, $"Join weekly event now", emailBody));
        }
    }
}
