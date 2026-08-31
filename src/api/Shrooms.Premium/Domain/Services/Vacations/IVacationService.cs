using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationService
    {
        /// <summary>
        /// Replaces every matched employee's entitlement from a payroll export.
        ///
        /// <paramref name="asOf"/> is the day the figures were measured — the
        /// payslip date, not the upload time. The whole balance calculation hangs
        /// off it: leave taken on or before it is already netted out of the
        /// figures, so only later leave is charged against them. Pass null to
        /// take a date from the export's own preamble.
        /// </summary>
        Task<VacationEntitlementImportDto> ImportEntitlementsAsync(
            Stream fileStream,
            string fileName,
            string asOf,
            UserAndOrganizationDto userOrg);

        Task<VacationAvailableDaysDto> GetAvailableDaysAsync(UserAndOrganizationDto userOrgDto);
    }
}
