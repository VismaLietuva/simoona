using Shrooms.Contracts.DataTransferObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailTemplate
    {
        /// <summary>
        /// Renders a template for the given model. Optionally applies a timezone offset to
        /// properties decorated with <c>ApplyTimeZoneChanges</c>.
        /// </summary>
        Task<string> GenerateAsync<T>(T viewModel, string key, string timeZoneKey = null)
            where T : BaseEmailTemplateViewModel;

        /// <summary>
        /// Renders one template per distinct timezone key, returning a grouped result.
        /// </summary>
        Task<ITimeZoneEmailGroup> GenerateAsync<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            IEnumerable<string> timeZoneKeys)
            where TEmailTemplate : BaseEmailTemplateViewModel;
    }
}
