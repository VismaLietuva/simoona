using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Participation
{
    public interface IEventParticipationService
    {
        Task AddColleagueAsync(EventJoinDTO joinDto);

        Task JoinAsync(EventJoinDTO joinDto, bool addedByColleague = false);

        Task UpdateAttendStatusAsync(UpdateAttendStatusDTO updateAttendStatusDTO);

        Task DeleteByEventAsync(Guid eventId, string userId);

        Task LeaveAsync(Guid eventId, UserAndOrganizationDTO userOrg, string leaveComment);

        Task ResetAttendeesAsync(Guid eventId, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<string>> GetParticipantsEmailsIncludingHostAsync(Guid eventId);

        Task ExpelAsync(Guid eventId, UserAndOrganizationDTO userOrg, string userId);

        Task<IEnumerable<EventParticipantDTO>> GetEventParticipantsAsync(Guid eventId, UserAndOrganizationDTO userAndOrg);

        Task<IEnumerable<EventUserSearchResultDTO>> SearchForEventJoinAutocompleteAsync(Guid eventId, string searchString, UserAndOrganizationDTO userOrg);

        Task<int> GetMaxParticipantsCountAsync(UserAndOrganizationDTO userAndOrganizationDTO);

        Task UpdateSelectedOptionsAsync(EventChangeOptionsDTO changeOptionsDTO);
    }
}
