using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class Event : ISoftDelete
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public EventRecurrenceOptions EventRecurring { get; set; }
        [ForeignKey("Office")]
        public int? OfficeId { get; set; }
        public virtual Office Office { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
        [Range(0, short.MaxValue)]
        public int MaxParticipants { get; set; }
        [Range(0, short.MaxValue)]
        public int MaxChoices { get; set; }
        public int EventTypeId { get; set; }
        public virtual EventType EventType { get; set; }
        public string ResponsibleUserId { get; set; }
        public virtual ApplicationUser ResponsibleUser { get; set; }
        [ForeignKey("Wall")]
        public int WallId { get; set; }
        public virtual Wall Wall { get; set; }
        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
        public virtual ICollection<EventOption> EventOptions { get; set; }
        public bool IsPinned { get; set; }
        [Required]
        public string Offices { get; set; }

        [NotMapped]
        public IEnumerable<string> OfficeIds
        {
            get { return Offices == null ? null : JsonConvert.DeserializeObject<string[]>(Offices); }
            set { Offices = JsonConvert.SerializeObject(value); }
        }

        [NotMapped]
        public DateTime LocalStartDate
        {
            get => GetLocalDateFromUtcDate(StartDate);
            set => StartDate = GetUtcDateFromLocalDate(value);
        }

        [NotMapped]
        public DateTime LocalEndDate
        {
            get => GetLocalDateFromUtcDate(EndDate);
            set => EndDate = GetUtcDateFromLocalDate(value);
        }

        [NotMapped]
        public DateTime LocalRegistrationDeadline
        {
            get => GetLocalDateFromUtcDate(RegistrationDeadline);
            set => RegistrationDeadline = GetUtcDateFromLocalDate(value);
        }

        private DateTime GetLocalDateFromUtcDate(DateTime utcDateTime) =>
            TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById(ResponsibleUser.TimeZone));

        private DateTime GetUtcDateFromLocalDate(DateTime localDateTime) =>
            TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.FindSystemTimeZoneById(ResponsibleUser.TimeZone));
    }
}
