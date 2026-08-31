using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationReportService
    {
        /// <summary>
        /// The payroll hand-off: approved leave whose period overlaps
        /// [<paramref name="from"/>, <paramref name="to"/>], as
        /// `name;dateFrom;dateTo;type`. Both bounds are calendar days; leave
        /// them empty for the month in progress.
        /// </summary>
        Task<VacationDocumentDto> GetReportAsync(string from, string to, UserAndOrganizationDto userOrg);

        /// <summary>
        /// The same file back in: every row becomes an approved request, except
        /// the ones already on file and the ones that cannot be placed. Nothing
        /// existing is touched, so a re-upload of the same report is a no-op.
        /// </summary>
        Task<VacationReportImportDto> ImportAsync(Stream fileStream, string fileName, UserAndOrganizationDto userOrg);
    }
}
