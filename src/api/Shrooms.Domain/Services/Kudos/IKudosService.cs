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
        Task CreateKudosType(NewKudosTypeDto dto);

        Task UpdateKudosTypeAsync(KudosTypeDTO dto);

        Task RemoveKudosTypeAsync(int id, UserAndOrganizationDTO userOrg);

        KudosTypeDTO GetSendKudosType(UserAndOrganizationDTO userOrg);

        Task<WelcomeKudosDTO> GetWelcomeKudosAsync();

        IEnumerable<KudosPieChartSliceDto> GetKudosPieChartData(int organizationId, string userId);

        IEnumerable<KudosTypeDTO> GetKudosTypes(UserAndOrganizationDTO userAndOrg);

        Task<KudosTypeDTO> GetKudosTypeAsync(int id, UserAndOrganizationDTO userOrg);

        IEnumerable<UserKudosInformationDTO> GetApprovedKudosList(string id, int organizationId);

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

        void AddRefundKudosLogs(IEnumerable<AddKudosLogDTO> kudosLogs);

        void UpdateProfilesFromUserIds(IEnumerable<string> usersId, UserAndOrganizationDTO userOrg);

        Task<KudosLogsEntriesDto<MainKudosLogDTO>> GetKudosLogsAsync(KudosLogsFilterDTO options);

        Task<KudosLogsEntriesDto<KudosUserLogDTO>> GetUserKudosLogsAsync(string userId, int page, int organizationId);

        int GetKudosTypeId(string kudosTypeName);

        Task<int> GetKudosTypeIdAsync(KudosTypeEnum kudosType);

        Task<IEnumerable<KudosBasicDataDTO>> GetKudosStatsAsync(int months, int amount, int organizationId);

        Task UpdateProfileKudosAsync(ApplicationUser user, UserAndOrganizationDTO userOrg);

        bool HasPendingKudos(string employeeId);
    }
}
