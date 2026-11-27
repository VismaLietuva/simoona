using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Infrastructure.Email.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Email
{
    public abstract class NotificationServiceBase
    {
        private readonly IApplicationSettings _settings;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IPermissionService _permissionService;
        private readonly string _receiverPermission;

        protected NotificationServiceBase(
            IApplicationSettings settings,
            IMailTemplate mailTemplate,
            IMailingService mailingService)
        {
            _settings = settings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
        }

        /// <param name="receiverPermission">
        /// Only users holding this permission get this service's emails, so a role that leaves a module out
        /// also leaves out that module's emails.
        /// </param>
        protected NotificationServiceBase(
            IApplicationSettings settings,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IPermissionService permissionService,
            string receiverPermission)
            : this(settings, mailTemplate, mailingService)
        {
            _permissionService = permissionService;
            _receiverPermission = receiverPermission;
        }

        protected virtual async Task SendSingleEmailAsync<TEmailTemplate>(
            string email,
            string subject,
            TEmailTemplate template,
            string templateCacheKey)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            await SendEmailToReceiversInternalAsync(new List<string> { email }, subject, template, templateCacheKey);
        }

        protected virtual async Task SendMultipleEmailsAsync<TEmailTemplate>(
            IEnumerable<string> emails,
            string subject,
            TEmailTemplate template,
            string templateCacheKey)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            await SendEmailToReceiversInternalAsync(emails, subject, template, templateCacheKey);
        }

        protected virtual async Task SendMultipleEmailsAsync<TEmailTemplate>(
            IEnumerable<IEmailReceiver> receivers,
            string subject,
            TEmailTemplate template,
            string templateCacheKey)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var receiverList = receivers.ToList();

            if (_receiverPermission != null && receiverList.Any())
            {
                var permittedEmails = (await _permissionService.FilterEmailsByPermissionAsync(receiverList.Select(r => r.Email), _receiverPermission))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                receiverList = receiverList.Where(r => r.Email != null && permittedEmails.Contains(r.Email)).ToList();
            }

            if (!receiverList.Any())
            {
                return;
            }

            var receiverTimeZoneGroup = receiverList.CreateTimeZoneGroup();
            var emailTimeZoneGroup = await _mailTemplate.GenerateAsync(template, templateCacheKey, receiverTimeZoneGroup.GetTimeZoneKeys());
            await _mailingService.SendEmailsAsync(emailTimeZoneGroup.CreateEmails(receiverTimeZoneGroup, subject));
        }

        protected virtual string CreateSubject(string format, params string[] values)
        {
            return string.Format(format, values);
        }

        protected virtual string GetNotificationSettingsUrl(string organizationShortName)
        {
            return GetNotificationSettingsUrlInternal(organizationShortName);
        }

        protected virtual string GetNotificationSettingsUrl(Organization organization)
        {
            return GetNotificationSettingsUrlInternal(organization.ShortName);
        }

        protected virtual string GetNotificationSettingsUrl(UserAndOrganizationHubDto hubDto)
        {
            return GetNotificationSettingsUrlInternal(hubDto.OrganizationName);
        }

        private async Task SendEmailToReceiversInternalAsync<TEmailTemplate>(
            IEnumerable<string> emails,
            string subject,
            TEmailTemplate template,
            string templateCacheKey)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var receivers = _receiverPermission == null
                ? emails.ToList()
                : await _permissionService.FilterEmailsByPermissionAsync(emails, _receiverPermission);

            if (!receivers.Any())
            {
                return;
            }

            var body = await _mailTemplate.GenerateAsync(template, templateCacheKey);
            await _mailingService.SendEmailAsync(new EmailDto(receivers, subject, body));
        }

        private string GetNotificationSettingsUrlInternal(string organizationShortName)
        {
            return _settings.UserNotificationSettingsUrl(organizationShortName);
        }
    }
}
