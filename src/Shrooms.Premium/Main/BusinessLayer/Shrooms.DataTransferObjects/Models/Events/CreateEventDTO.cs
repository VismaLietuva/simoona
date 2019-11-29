﻿using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class CreateEventDto : UserAndOrganizationDTO
    {
        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? RegistrationDeadlineDate { get; set; }

        public EventRecurrenceOptions Recurrence { get; set; }
        
        public EventOfficesDTO Offices { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int MaxParticipants { get; set; }

        public int MaxOptions { get; set; }

        public int TypeId { get; set; }

        public string ResponsibleUserId { get; set; }

        public bool ResetParticipantList { get; set; }

        public IEnumerable<string> NewOptions { get; set; }

        public string Id { get; set; }
    }
}
