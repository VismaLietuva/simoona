using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Infrastructure.Email.Extensions;
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

        protected NotificationServiceBase(
            IApplicationSettings settings,
            IMailTemplate mailTemplate,
            IMailingService mailingService)
        {
            _settings = settings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
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
            if (!receivers.Any())
            {
                return;
            }

            var receiverTimeZoneGroup = receivers.CreateTimeZoneGroup();
            var emailTimeZoneGroup = _mailTemplate.Generate(template, templateCacheKey, receiverTimeZoneGroup.GetTimeZoneKeys());
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
            if (!emails.Any())
            {
                return;
            }

            var body = _mailTemplate.Generate(template, templateCacheKey);
            await _mailingService.SendEmailAsync(new EmailDto(emails, subject, body));
        }

        private string GetNotificationSettingsUrlInternal(string organizationShortName)
        {
            return _settings.UserNotificationSettingsUrl(organizationShortName);
        }
    }
}
