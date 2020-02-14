using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Vacations
{
    public interface IVacationService
    {
        VacationImportStatusDTO UploadVacationReportFile(Stream fileStream);
        Task<VacationAvailableDaysDTO> GetAvailableDays(UserAndOrganizationDTO userOrgDto);
    }
}