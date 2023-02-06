using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Email.Converters
{
    public interface IMailTemplateConverter
    {
        Task<IEnumerable<CompiledEmailTemplateWithReceiverEmails>> ConvertEmailTemplateToReceiversTimeZoneSettingsAsync<TEmailTemplate>(
            TEmailTemplate emailTemplate,
            string emailCacheKey,
            IEnumerable<IEmailReceiver> receivers,
            params Expression<Func<TEmailTemplate, DateTime>>[] utcDateTimePropertiesForTimeZoneChanges)
            where TEmailTemplate : BaseEmailTemplateViewModel;
    }
}
