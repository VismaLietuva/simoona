using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.Domain.Services.Events.Participation
{
    public interface IEventParticipationService
    {
        void AddColleague(EventJoinDTO joinDto);

        void Join(EventJoinDTO joinDto, bool addedByColleague = false);

        void UpdateAttendStatus(UpdateAttendStatusDTO updateAttendStatusDTO);

        void DeleteByEvent(Guid eventId, string userId);

        void Leave(Guid eventId, UserAndOrganizationDTO userOrg, string leaveComment);

        void ResetAttendees(Guid eventId, UserAndOrganizationDTO userOrg);

        IEnumerable<string> GetParticipantsEmailsIncludingHost(Guid eventId);

        void Expel(Guid eventId, UserAndOrganizationDTO userOrg, string userId);

        IEnumerable<EventParticipantDTO> GetEventParticipants(Guid eventId, UserAndOrganizationDTO userAndOrg);

        IEnumerable<EventUserSearchResultDTO> SearchForEventJoinAutocomplete(Guid eventId, string searchString, UserAndOrganizationDTO userOrg);

        int GetMaxParticipantsCount(UserAndOrganizationDTO userAndOrganizationDTO);

        void UpdateSelectedOptions(EventChangeOptionsDTO changeOptionsDTO);
    }
}
