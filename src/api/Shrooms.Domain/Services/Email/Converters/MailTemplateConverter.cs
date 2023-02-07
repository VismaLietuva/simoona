using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Domain.Services.Email.Converters
{
    public class MailTemplateConverter : IMailTemplateConverter
    {
        private readonly IMailTemplate _mailTemplate;

        public MailTemplateConverter(IMailTemplate mailTemplate)
        {
            _mailTemplate = mailTemplate;
        }

        public async Task<IEnumerable<CompiledEmailTemplateWithReceiverEmails>> ConvertEmailTemplateToReceiversTimeZoneSettingsAsync<TEmailTemplate>(
            TEmailTemplate emailTemplate,
            string emailCacheKey,
            IEnumerable<IEmailReceiver> receivers,
            params Expression<Func<TEmailTemplate, DateTime>>[] utcDateTimePropertiesForTimeZoneChanges)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            if (!receivers.Any())
            {
                return new List<CompiledEmailTemplateWithReceiverEmails>();
            }

            var templateLock = new object();
            var emailTemplateType = emailTemplate.GetType();

            var dateTimePropertiesWithInitialValues = await utcDateTimePropertiesForTimeZoneChanges.Select(dateTimeFunc =>
            {
                var memberExpr = (MemberExpression)dateTimeFunc.Body;
                var propertyName = memberExpr.Member.Name;
                var property = emailTemplateType.GetProperty(propertyName);
                var initialValue = property.GetValue(emailTemplate);
                return (Property: property, InitialValue: (DateTime)initialValue);
            }).ToListAsync();

            var receiversGroupedByTimeZone = await receivers.GroupBy(receiver => receiver.TimeZoneKey, receiver => receiver.Email)
                .ToListAsync();

            var compiledTemplatesParallelQuery = receiversGroupedByTimeZone.AsParallel()
                .Select(async receiver =>
                {
                    lock (templateLock)
                    {
                        foreach (var (property, initialValue) in dateTimePropertiesWithInitialValues)
                        {
                            var date = initialValue;
                            var dateWithAppliedTimeZone = date.ConvertUtcToTimeZone(receiver.Key);
                            property.SetValue(emailTemplate, dateWithAppliedTimeZone);
                        }
                    }

                    var emailBody = _mailTemplate.Generate(emailTemplate, emailCacheKey);
                    var compiledTemplate = new CompiledEmailTemplateWithReceiverEmails
                    {
                        Body = emailBody,
                        ReceiverEmails = await receiver.ToListAsync()
                    };
                    return compiledTemplate;
                });
            var compiledTemplates = await Task.WhenAll(compiledTemplatesParallelQuery);

            // Restore initial values to passed in email template
            foreach (var (property, initialValue) in dateTimePropertiesWithInitialValues)
            {
                property.SetValue(emailTemplate, initialValue);
            }

            return compiledTemplates;
        }
    }
}
