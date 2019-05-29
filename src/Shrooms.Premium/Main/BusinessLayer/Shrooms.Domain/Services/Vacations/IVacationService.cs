using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Vacations;
using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Vacations
{
    public interface IVacationService
    {
        VacationImportStatusDTO UploadVacationReportFile(Stream fileStream);
        Task<VacationAvailableDaysDTO> GetAvailableDays(UserAndOrganizationDTO userOrgDto);
    }
}