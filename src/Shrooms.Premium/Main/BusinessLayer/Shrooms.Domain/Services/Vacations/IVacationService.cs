using System.IO;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Vacations
{
    public interface IVacationService
    {
        VacationImportStatusDTO UploadVacationReportFile(Stream fileStream);
        Task<VacationAvailableDaysDTO> GetAvailableDays(UserAndOrganizationDTO userOrgDto);
    }
}