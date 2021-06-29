using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class CreateEventDto : UserAndOrganizationDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? RegistrationDeadlineDate { get; set; }

        public EventRecurrenceOptions Recurrence { get; set; }

        public bool AllowMaybeGoing { get; set; }
        public bool AllowNotGoing { get; set; }

        public EventOfficesDTO Offices { get; set; }

        public bool IsPinned { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int MaxParticipants { get; set; }

        public int MaxOptions { get; set; }

        public int TypeId { get; set; }

        public string ResponsibleUserId { get; set; }

        public bool ResetParticipantList { get; set; }

        public IEnumerable<NewEventOptionDTO> NewOptions { get; set; }
    }
}