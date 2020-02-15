using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationService
    {
        VacationImportStatusDTO UploadVacationReportFile(Stream fileStream);
        Task<VacationAvailableDaysDTO> GetAvailableDays(UserAndOrganizationDTO userOrgDto);
    }
}