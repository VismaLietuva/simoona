using Shrooms.EntityModels.Models;
using System;
using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventDetailsDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDeadlineDate { get; set; }
        public EventOfficesDTO Offices { get; set; }
        public bool IsPinned { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int MaxParticipants { get; set; }
        public int MaxOptions { get; set; }
        public string HostUserFullName { get; set; }
        public string HostUserId { get; set; }
        public bool IsFull { get; set; }
        public int ParticipatingStatus { get; set; }
        public int WallId { get; set; }
        public int? FoodOption { get; set; }
        public IEnumerable<EventDetailsOptionDTO> Options { get; set; }
        public IEnumerable<EventDetailsParticipantDTO> Participants { get; set; }
    }
}
