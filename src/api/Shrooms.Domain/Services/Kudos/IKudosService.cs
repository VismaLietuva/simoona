using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosService
    {
        Task CreateKudosTypeAsync(NewKudosTypeDto dto);

        Task UpdateKudosTypeAsync(KudosTypeDTO dto);

        Task RemoveKudosTypeAsync(int id, UserAndOrganizationDTO userOrg);

        Task<KudosTypeDTO> GetSendKudosTypeAsync(UserAndOrganizationDTO userOrg);

        Task<WelcomeKudosDTO> GetWelcomeKudosAsync();

        Task<IEnumerable<KudosPieChartSliceDto>> GetKudosPieChartDataAsync(int organizationId, string userId);

        Task<IEnumerable<KudosTypeDTO>> GetKudosTypesAsync(UserAndOrganizationDTO userAndOrg);

        Task<KudosTypeDTO> GetKudosTypeAsync(int id, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<UserKudosInformationDTO>> GetApprovedKudosListAsync(string id, int organizationId);

        Task AddLotteryKudosLogAsync(AddKudosLogDTO kudosLogDTO, UserAndOrganizationDTO userOrg);

        Task ApproveKudosAsync(int kudosLogId, UserAndOrganizationDTO userOrg);

        Task RejectKudosAsync(KudosRejectDTO kudosRejectDTO);

        Task<UserKudosDTO> GetUserKudosInformationByIdAsync(string id, int organizationId);

        Task<IEnumerable<WallKudosLogDTO>> GetLastKudosLogsForWallAsync(UserAndOrganizationDTO userAndOrg);

        Task<decimal[]> GetMonthlyKudosStatisticsAsync(string id);

        /// <summary>
        /// Adds kudos request where points are not calculated
        /// </summary>
        /// <param name="kudosDto">add kudos request</param>
        /// <param name="points">requested points</param>
        Task AddKudosLogAsync(AddKudosLogDTO kudosDto, decimal? points = null);

        Task AddRefundKudosLogsAsync(IEnumerable<AddKudosLogDTO> kudosLogs);

        Task UpdateProfilesFromUserIdsAsync(IEnumerable<string> usersId, UserAndOrganizationDTO userOrg);

        Task<KudosLogsEntriesDto<MainKudosLogDTO>> GetKudosLogsAsync(KudosLogsFilterDTO options);

        Task<KudosLogsEntriesDto<KudosUserLogDTO>> GetUserKudosLogsAsync(string userId, int page, int organizationId);

        Task<int> GetKudosTypeIdAsync(string kudosTypeName);

        Task<int> GetKudosTypeIdAsync(KudosTypeEnum kudosType);

        Task<IEnumerable<KudosBasicDataDTO>> GetKudosStatsAsync(int months, int amount, int organizationId);

        Task UpdateProfileKudosAsync(ApplicationUser user, UserAndOrganizationDTO userOrg);

        Task<bool> HasPendingKudosAsync(string employeeId);
    }
}
