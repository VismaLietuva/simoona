using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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

        public EventNotificationService(IUnitOfWork2 uow,
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

        public async Task RemindUsersToJoinEventAsync(IEnumerable<EventTypeDTO> eventTypes, IEnumerable<string> emails, int orgId)
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
    }
}
