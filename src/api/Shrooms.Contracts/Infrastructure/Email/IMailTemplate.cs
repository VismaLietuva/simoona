using Shrooms.Contracts.DataTransferObjects;
using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailTemplate
    {
        string Generate<T>(T viewModel, string key, string timeZoneKey = null)
            where T : BaseEmailTemplateViewModel;

        ITimeZoneEmailGroup Generate<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            IEnumerable<string> timeZoneKeys)
            where TEmailTemplate : BaseEmailTemplateViewModel;
    }
}