using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Leave orders: the numbered document that grants approved leave. Numbers
    /// are allocated here and never reused, so a reprint reproduces the paper
    /// that was signed.
    /// </summary>
    public interface IVacationOrderService
    {
        /// <summary>
        /// Newest first. The period filters on the leave the order grants, not on
        /// the order's own date — one signed on 31 July for leave starting 3
        /// August belongs to August. Leave both bounds empty for every order.
        /// </summary>
        Task<IList<VacationOrderDto>> GetOrdersAsync(string from, string to, UserAndOrganizationDto userOrg);

        /// <summary>
        /// One order per start day per leave type, over the approved leave
        /// beginning inside the period, each dated the last working day before
        /// that leave starts. Re-running it keeps every number and refreshes the
        /// lines from what is approved now.
        /// </summary>
        Task<VacationOrderGenerationDto> GenerateAsync(string from, string to, UserAndOrganizationDto userOrg);

        Task<VacationDocumentDto> GetOrderDocumentAsync(int id, UserAndOrganizationDto userOrg);

        /// <summary>Every document of the period, zipped.</summary>
        Task<VacationDocumentDto> GetArchiveAsync(string from, string to, UserAndOrganizationDto userOrg);
    }
}
