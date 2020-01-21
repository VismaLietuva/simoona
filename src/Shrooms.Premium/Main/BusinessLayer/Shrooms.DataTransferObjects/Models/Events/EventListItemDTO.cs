﻿using System;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventListItemDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        public EventOfficesDTO Offices { get; set; }

        public bool IsPinned { get; set; }

        public string Place { get; set; }

        public int MaxParticipants { get; set; }

        public int ParticipantsCount { get; set; }

        public bool IsCreator { get; set; }

        public int ParticipatingStatus { get; set; }

        public int MaxChoices { get; set; }
    }
}