using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Events;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;

namespace Shrooms.Premium.Domain.Services.Email.Event
{
    public class EventNotificationService : NotificationServiceBase, IEventNotificationService
    {
        private static readonly Regex PartialShortcode = new(@"(?<=^|[^\p{L}\p{N}\p{M}:]|:[a-z0-9_+-]{1,60}:):[a-z0-9_+-]*$", RegexOptions.Compiled);

        private static readonly Regex UrlStart = new(@"(?:https?|ftp)://|mailto:|tel:|www\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IEmailMarkdownRenderer _emailMarkdownRenderer;

        private readonly DbSet<ApplicationUser> _usersDbSet;

        public EventNotificationService(
            IUnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService,
            IEmailMarkdownRenderer emailMarkdownRenderer)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;
            _organizationService = organizationService;
            _emailMarkdownRenderer = emailMarkdownRenderer;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task NotifyRemovedEventParticipantsAsync(string eventName, Guid eventId, int orgId, IEnumerable<string> users)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(orgId);
            var emails = await _usersDbSet
                .Where(u => users.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();
            var eventUrl = _appSettings.EventUrl(organization.ShortName, eventId.ToString());
            var emailTemplateViewModel = new EventParticipantExpelledEmailTemplateViewModel(GetNotificationSettingsUrl(organization), eventName, eventUrl);

            await SendMultipleEmailsAsync(emails,
                Resources.Models.Events.Events.ResetParticipantListEmailSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.EventParticipantExpelled);
        }

        public async Task RemindUsersAboutDeadlineDateOfJoinedEventsAsync(IEnumerable<EventReminderDeadlineEmailDto> deadlineEmailDtos, Organization organization)
        {
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            foreach (var deadlineEmailDto in deadlineEmailDtos)
            {
                await RemindUsersAboutDeadlineDateOfJoinedEventAsync(deadlineEmailDto, organization, userNotificationSettingsUrl);
            }
        }

        public async Task RemindUsersAboutStartDateOfJoinedEventsAsync(IEnumerable<EventReminderStartEmailDto> startEmailDtos, Organization organization)
        {
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            foreach (var startEmailDto in startEmailDtos)
            {
                await RemindUsersAboutStartDateOfJoinedEventAsync(startEmailDto, organization, userNotificationSettingsUrl);
            }
        }

        public async Task RemindUsersToJoinEventAsync(IEnumerable<EventTypeDto> eventTypes, IEnumerable<string> emails, int orgId)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(orgId);
            var emailTemplateViewModel = new EventJoinRemindEmailTemplateViewModel(GetNotificationSettingsUrl(organization));
            foreach (var eventType in eventTypes)
            {
                emailTemplateViewModel.EventTypes.Add(eventType.Name, _appSettings.EventListByTypeUrl(organization.ShortName, eventType.Id.ToString()));
            }

            await SendMultipleEmailsAsync(emails, "Join weekly event now", emailTemplateViewModel, EmailPremiumTemplateCacheKeys.EventJoinRemind);
        }

        public async Task NotifyManagerAboutEventAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto, bool isJoiningEvent)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(userAttendStatusDto.OrganizationId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, userAttendStatusDto.EventId.ToString());
            await SendManagerNotifyEmailAsync(userAttendStatusDto, userNotificationSettingsUrl, eventUrl, isJoiningEvent);
        }

        public async Task NotifySharedEventAsync(SharedEventEmailDto shareEventEmailDto, UserAndOrganizationHubDto userOrgHubDto)
        {
            var postUrl = _appSettings.WallPostUrl(userOrgHubDto.OrganizationName, shareEventEmailDto.CreatedPost.Id);
            var eventUrl = _appSettings.EventUrl(userOrgHubDto.OrganizationName, shareEventEmailDto.CreatedPost.SharedEventId);
            var subject = CreateSubject(Resources.Models.Events.Events.ShareEventEmailSubject, shareEventEmailDto.Details.Name, shareEventEmailDto.CreatedPost.WallName);
            var body = await RenderAsync(shareEventEmailDto.CreatedPost.MessageBody, userOrgHubDto);
            var description = await RenderTruncatedAsync(shareEventEmailDto.Details.Description, userOrgHubDto);
            var emailTemplate = new SharedEventEmailTemplateViewModel(
                postUrl,
                eventUrl,
                shareEventEmailDto.CreatedPost.User.FullName,
                body,
                shareEventEmailDto.CreatedPost.WallName,
                shareEventEmailDto.Details.Name,
                shareEventEmailDto.Details.StartDate,
                description,
                shareEventEmailDto.Details.Location,
                GetNotificationSettingsUrl(userOrgHubDto));

            await SendMultipleEmailsAsync(shareEventEmailDto.Receivers, subject, emailTemplate, EmailPremiumTemplateCacheKeys.EventShared);
        }

        public async Task NotifyNewEventAsync(CreateEventDto eventArgsDto, IEnumerable<IEmailReceiver> receivers, UserAndOrganizationHubDto userOrgHubDto)
        {
            var eventUrl = _appSettings.EventUrl(userOrgHubDto.OrganizationName, eventArgsDto.Id);
            var subject = CreateSubject(Resources.Models.Events.Events.NewEventEmailSubject, eventArgsDto.Name);
            var template = new NewEventEmailTemplateViewModel(
                eventUrl,
                eventArgsDto.Name,
                await RenderTruncatedAsync(eventArgsDto.Description, userOrgHubDto),
                eventArgsDto.Location,
                eventArgsDto.StartDate,
                GetNotificationSettingsUrl(userOrgHubDto));

            await SendMultipleEmailsAsync(
                receivers,
                subject,
                template,
                EmailPremiumTemplateCacheKeys.EventNew);
        }

        private async Task SendManagerNotifyEmailAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto, string userNotificationSettingsUrl, string eventUrl, bool isJoiningEvent)
        {
            if (!isJoiningEvent)
            {
                await SendCoacheeLeftManagerEmailAsync(userAttendStatusDto, userNotificationSettingsUrl, eventUrl);
            }
            else
            {
                await SendCoacheeJoinedManagerEmailAsync(userAttendStatusDto, userNotificationSettingsUrl, eventUrl);
            }
        }

        private async Task SendCoacheeJoinedManagerEmailAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto, string userNotificationSettingsUrl, string eventUrl)
        {
            var emailTemplateJoinViewModel = new CoacheeJoinedEventEmailTemplateViewModel(
                userNotificationSettingsUrl,
                userAttendStatusDto,
                eventUrl);
            var emailJoinSubject = CreateSubject(
                Resources.Models.Events.Events.CoacheeJoinedEventEmailSubject,
                userAttendStatusDto.FullName,
                userAttendStatusDto.EventName);

            await SendMultipleEmailsAsync(
                new List<string> { userAttendStatusDto.ManagerEmail },
                emailJoinSubject,
                emailTemplateJoinViewModel,
                EmailPremiumTemplateCacheKeys.CoacheeJoinedEvent);
        }

        private async Task SendCoacheeLeftManagerEmailAsync(UserEventAttendStatusChangeEmailDto userAttendStatusDto, string userNotificationSettingsUrl, string eventUrl)
        {
            var emailTemplateLeaveViewModel = new CoacheeLeftEventEmailTemplateViewModel(
                userNotificationSettingsUrl,
                userAttendStatusDto,
                eventUrl);
            var emailLeaveSubject = CreateSubject(
                Resources.Models.Events.Events.CoacheeLeftEventEmailSubject,
                userAttendStatusDto.FullName,
                userAttendStatusDto.EventName);

            await SendMultipleEmailsAsync(
                new List<string> { userAttendStatusDto.ManagerEmail },
                emailLeaveSubject,
                emailTemplateLeaveViewModel,
                EmailPremiumTemplateCacheKeys.CoacheeLeftEvent);
        }

        private async Task RemindUsersAboutDeadlineDateOfJoinedEventAsync(EventReminderDeadlineEmailDto deadlineEmailDto, Organization organization, string userNotificationSettingsUrl)
        {
            var subject = CreateSubject(Resources.Models.Events.Events.RemindEventDeadlineEmailSubject, deadlineEmailDto.EventName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, deadlineEmailDto.EventId.ToString());
            var emailTemplate = new EventReminderDeadlineEmailTemplateViewModel(
                userNotificationSettingsUrl,
                deadlineEmailDto.EventName,
                eventUrl,
                deadlineEmailDto.StartDate,
                deadlineEmailDto.DeadlineDate);

            await SendMultipleEmailsAsync(deadlineEmailDto.Receivers, subject, emailTemplate, EmailPremiumTemplateCacheKeys.EventDeadlineRemind);
        }

        private async Task RemindUsersAboutStartDateOfJoinedEventAsync(EventReminderStartEmailDto startEmailDto, Organization organization, string userNotificationSettingsUrl)
        {
            var subject = CreateSubject(Resources.Models.Events.Events.RemindEventStartEmailSubject, startEmailDto.EventName);
            var eventUrl = _appSettings.EventUrl(organization.ShortName, startEmailDto.EventId.ToString());
            var emailTemplate = new EventReminderStartEmailTemplateViewModel(
                userNotificationSettingsUrl,
                startEmailDto.EventName,
                eventUrl,
                startEmailDto.StartDate);

            await SendMultipleEmailsAsync(startEmailDto.Receivers, subject, emailTemplate, EmailPremiumTemplateCacheKeys.EventStartRemind);
        }

        private Task<string> RenderAsync(string markdown, UserAndOrganizationHubDto userOrgHubDto)
        {
            return _emailMarkdownRenderer.RenderAsync(markdown, userOrgHubDto.OrganizationId, userOrgHubDto.OrganizationName);
        }

        private Task<string> RenderTruncatedAsync(string markdown, UserAndOrganizationHubDto userOrgHubDto, int maxCharacterCount = 150)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return Task.FromResult<string>(null);
            }

            return RenderAsync(Truncate(markdown, maxCharacterCount), userOrgHubDto);
        }

        private static string Truncate(string markdown, int maxCharacterCount)
        {
            if (markdown.Length <= maxCharacterCount)
            {
                return markdown;
            }

            var cut = KeepUrlWhole(markdown, LastTextElementBoundary(markdown, maxCharacterCount));

            return $"{PartialShortcode.Replace(markdown[..cut], string.Empty)}...";
        }

        private static int LastTextElementBoundary(string text, int maxLength)
        {
            var boundary = 0;
            while (boundary < text.Length)
            {
                var next = boundary + StringInfo.GetNextTextElementLength(text, boundary);
                if (next > maxLength)
                {
                    break;
                }

                boundary = next;
            }

            return boundary;
        }

        private static int KeepUrlWhole(string text, int cut)
        {
            if (cut == 0 || cut >= text.Length || char.IsWhiteSpace(text[cut - 1]) || char.IsWhiteSpace(text[cut]))
            {
                return cut;
            }

            var wordStart = cut;
            while (wordStart > 0 && !char.IsWhiteSpace(text[wordStart - 1]))
            {
                wordStart--;
            }

            var wordEnd = cut;
            while (wordEnd < text.Length && !char.IsWhiteSpace(text[wordEnd]))
            {
                wordEnd++;
            }

            var url = UrlStart.Match(text[wordStart..wordEnd]);
            if (!url.Success || wordStart + url.Index >= cut)
            {
                return cut;
            }

            return wordStart > 0 ? wordStart : wordEnd;
        }
    }
}
