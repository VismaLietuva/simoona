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

        Task UpdateKudosTypeAsync(KudosTypeDto dto);

        Task RemoveKudosTypeAsync(int id, UserAndOrganizationDto userOrg);

        Task<KudosTypeDto> GetSendKudosTypeAsync(UserAndOrganizationDto userOrg);

        Task<WelcomeKudosDto> GetWelcomeKudosAsync();

        Task<IEnumerable<KudosPieChartSliceDto>> GetKudosPieChartDataAsync(int organizationId, string userId);

        Task<IEnumerable<KudosTypeDto>> GetKudosTypesAsync(UserAndOrganizationDto userAndOrg);

        Task<KudosTypeDto> GetKudosTypeAsync(int id, UserAndOrganizationDto userOrg);

        Task<IEnumerable<UserKudosInformationDto>> GetApprovedKudosListAsync(string id, int organizationId);

        Task AddLotteryKudosLogAsync(AddKudosLogDto kudosLogDto, UserAndOrganizationDto userOrg);

        Task ApproveKudosAsync(int kudosLogId, UserAndOrganizationDto userOrg);

        Task RejectKudosAsync(KudosRejectDto kudosRejectDto);

        Task<UserKudosDto> GetUserKudosInformationByIdAsync(string id, int organizationId);

        Task<IEnumerable<WallKudosLogDto>> GetLastKudosLogsForWallAsync(UserAndOrganizationDto userAndOrg);

        Task<decimal[]> GetMonthlyKudosStatisticsAsync(string id);

        /// <summary>
        /// Adds kudos request where points are not calculated
        /// </summary>
        /// <param name="kudosDto">add kudos request</param>
        /// <param name="points">requested points</param>
        Task AddKudosLogAsync(AddKudosLogDto kudosDto, decimal? points = null);

        Task AddRefundKudosLogsAsync(IEnumerable<AddKudosLogDto> kudosLogs);

        Task UpdateProfilesFromUserIdsAsync(IEnumerable<string> usersId, UserAndOrganizationDto userOrg);

        Task<KudosLogsEntriesDto<MainKudosLogDto>> GetKudosLogsAsync(KudosLogsFilterDto options);

        Task<KudosLogsEntriesDto<KudosUserLogDto>> GetUserKudosLogsAsync(string userId, int page, int organizationId);

        Task<int> GetKudosTypeIdAsync(string kudosTypeName);

        Task<int> GetKudosTypeIdAsync(KudosTypeEnum kudosType);

        Task<IEnumerable<KudosBasicDataDto>> GetKudosStatsAsync(int months, int amount, int organizationId);

        Task UpdateProfileKudosAsync(ApplicationUser user, UserAndOrganizationDto userOrg);

        Task<bool> HasPendingKudosAsync(string employeeId);
    }
}
