using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationSettingsService
    {
        Task<VacationSettingsDto> GetAsync(UserAndOrganizationDto userOrg);

        Task<VacationSettingsDto> UpdateAsync(VacationSettingsDto settings, UserAndOrganizationDto userOrg);
    }
}
