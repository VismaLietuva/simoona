﻿using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.Participation
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
