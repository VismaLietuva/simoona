using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events.Participation
{
    public interface IEventParticipationService
    {
        Task AddColleagueAsync(EventJoinDto joinDto);

        Task JoinAsync(EventJoinDto joinDto, bool addedByColleague = false);

        Task UpdateAttendStatusAsync(UpdateAttendStatusDto updateAttendStatusDto);

        Task DeleteByEventAsync(Guid eventId, string userId);

        Task LeaveAsync(Guid eventId, UserAndOrganizationDto userOrg, string leaveComment);

        Task ResetAllAttendeesAsync(Guid eventId, UserAndOrganizationDto userOrg);

        Task ResetVirtualAttendeesAsync(Event @event, UserAndOrganizationDto userOrg);

        Task ResetAttendeesAsync(Event @event, UserAndOrganizationDto userOrg);

        Task<IEnumerable<string>> GetParticipantsEmailsIncludingHostAsync(Guid eventId);

        Task ExpelAsync(Guid eventId, UserAndOrganizationDto userOrg, string userId);

        Task<IEnumerable<EventParticipantDto>> GetEventParticipantsAsync(Guid eventId, UserAndOrganizationDto userAndOrg);

        Task<IEnumerable<EventUserSearchResultDto>> SearchForEventJoinAutocompleteAsync(Guid eventId, string searchString, UserAndOrganizationDto userOrg);

        Task<int> GetMaxParticipantsCountAsync(UserAndOrganizationDto userAndOrganizationDto);

        Task UpdateSelectedOptionsAsync(EventChangeOptionsDto changeOptionsDto);
    }
}
