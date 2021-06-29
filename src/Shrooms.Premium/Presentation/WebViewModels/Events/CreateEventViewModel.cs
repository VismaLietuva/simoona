using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hangfire.Annotations;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(EventsConstants.EventNameMaxLength)]
        public string Name { get; set; }

        [Required]
        public string ImageName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime? RegistrationDeadlineDate { get; set; }

        [Required]
        public EventRecurrenceOptions Recurrence { get; set; }

        public bool AllowMaybeGoing { get; set; }
        public bool AllowNotGoing { get; set; }

        [Required]
        public List<int> Offices { get; set; }

        [Required]
        public bool IsPinned { get; set; }

        [Required]
        [StringLength(EventsConstants.EventLocationMaxLength)]
        public string Location { get; set; }

        [StringLength(EventsConstants.EventDescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        [Range(EventsConstants.EventMinimumParticipants, EventsConstants.EventMaxParticipants)]
        public int MaxParticipants { get; set; }

        [Range(EventsConstants.EventMinimumOptions, short.MaxValue)]
        public int MaxOptions { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public string ResponsibleUserId { get; set; }

        public IEnumerable<NewEventOptionViewModel> NewOptions { get; set; }
    }
}
