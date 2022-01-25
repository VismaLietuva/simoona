using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.VacationPages;

namespace Shrooms.Domain.Services.VacationPages
{
    public interface IVacationPageService
    {
        Task<VacationPageDto> GetVacationPage(int organizationId);
        Task EditVacationPage(UserAndOrganizationDto userAndOrg, VacationPageDto vacationPageDto);
    }
}