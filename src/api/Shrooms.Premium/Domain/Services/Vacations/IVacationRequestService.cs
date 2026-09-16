using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Everything that changes a request, plus the balance those changes are
    /// measured against. Every action writes exactly one audit event in the same
    /// save, so the register and the log cannot drift apart.
    /// </summary>
    public interface IVacationRequestService
    {
        Task<VacationBalanceDto> GetBalanceAsync(UserAndOrganizationDto userOrg);

        Task<VacationRequestDto> SubmitAsync(VacationRequestDraftDto draft, UserAndOrganizationDto userOrg);

        Task<VacationRequestDto> EditAsync(int id, VacationRequestDraftDto draft, UserAndOrganizationDto userOrg);

        Task<VacationRequestDto> CancelAsync(int id, UserAndOrganizationDto userOrg);

        /// <summary>The one request a reviewer is about to act on, for the approve and reject links in their mail.</summary>
        Task<VacationRequestDto> GetForReviewAsync(int id, UserAndOrganizationDto userOrg);

        Task<VacationRequestDto> ApproveAsync(int id, UserAndOrganizationDto userOrg);

        Task<VacationRequestDto> RejectAsync(int id, string reason, UserAndOrganizationDto userOrg);

        /// <summary>
        /// Administrator override: sets any field, status included, with none of
        /// the date rules that bind a requester.
        /// </summary>
        Task<VacationRequestDto> AdminEditAsync(int id, VacationAdminPatchDto patch, UserAndOrganizationDto userOrg);
    }
}
