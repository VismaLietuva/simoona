using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Kudos
{
    public interface IKudosService
    {
        Task CreateKudosType(NewKudosTypeDto dto);

        Task UpdateKudosType(KudosTypeDTO dto);

        Task RemoveKudosType(int id, UserAndOrganizationDTO userOrg);

        KudosTypeDTO GetSendKudosType(UserAndOrganizationDTO userOrg);

        WelcomeKudosDTO GetWelcomeKudos();

        IEnumerable<KudosPieChartSliceDto> GetKudosPieChartData(int organizationId, string userId);

        IEnumerable<KudosTypeDTO> GetKudosTypes(UserAndOrganizationDTO userAndOrg);

        Task<KudosTypeDTO> GetKudosType(int id, UserAndOrganizationDTO userOrg);

        IEnumerable<UserKudosInformationDTO> GetApprovedKudosList(string id, int organizationId);

        IEnumerable<UserKudosAutocompleteDTO> GetUsersForAutocomplete(string s);

        void ApproveKudos(int kudosLogId, UserAndOrganizationDTO userOrg);

        void RejectKudos(KudosRejectDTO kudosRejectDTO);

        UserKudosDTO GetUserKudosInformationById(string id, int organizationId);

        IEnumerable<WallKudosLogDTO> GetLastKudosLogsForWall(UserAndOrganizationDTO userAndOrg);

        decimal[] GetMonthlyKudosStatistics(string id);

        void AddKudosLog(AddKudosLogDTO kudosLog);

        /// <summary>
        /// Adds kudos request where points are not calculated
        /// </summary>
        /// <param name="kudosDto">add kudos request</param>
        /// <param name="points">requested points</param>
        void AddKudosLog(AddKudosLogDTO kudosDto, decimal points);

        void RefundLotteryTickets(IEnumerable<AddKudosLogDTO> kudosLogDTO, UserAndOrganizationDTO userOrg);

        KudosLogsEntriesDto<MainKudosLogDTO> GetKudosLogs(KudosLogsFilterDTO options);

        KudosLogsEntriesDto<KudosUserLogDTO> GetUserKudosLogs(string userId, int page, int organizationId);

        int GetKudosTypeId(string kudosTypeName);

        IEnumerable<KudosBasicDataDTO> GetKudosStats(int months, int amount, int organizationId);

        void UpdateProfileKudos(ApplicationUser user, UserAndOrganizationDTO userOrg);

        bool HasPendingKudos(string employeeId);

    }
}
