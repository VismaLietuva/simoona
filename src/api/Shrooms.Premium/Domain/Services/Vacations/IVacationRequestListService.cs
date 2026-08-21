using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// The three request listings. Reads only — every mutation goes through
    /// <see cref="IVacationRequestService"/>.
    /// </summary>
    public interface IVacationRequestListService
    {
        Task<IPagedList<VacationRequestDto>> GetMyRequestsAsync(VacationRequestArgsDto args);

        Task<IPagedList<VacationRequestDto>> GetTeamRequestsAsync(VacationRequestArgsDto args);

        Task<IPagedList<VacationRequestDto>> GetAllRequestsAsync(VacationRequestArgsDto args);

        Task<int> GetPendingTeamCountAsync(UserAndOrganizationDto userOrg);

        Task<bool> HasDirectReportsAsync(UserAndOrganizationDto userOrg);
    }
}
