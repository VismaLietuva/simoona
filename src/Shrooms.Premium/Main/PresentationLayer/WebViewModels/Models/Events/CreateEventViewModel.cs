using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(WebApiConstants.EventNameMaxLength)]
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

        public int? OfficeId { get; set; }

        [Required]
        [StringLength(WebApiConstants.EventLocationMaxLength)]
        public string Location { get; set; }

        [StringLength(WebApiConstants.EventDescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        [Range(WebApiConstants.EventMinimumParticipants, WebApiConstants.EventMaxParticipants)]
        public int MaxParticipants { get; set; }

        [Range(WebApiConstants.EventMinimumOptions, short.MaxValue)]
        public int MaxOptions { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        public string ResponsibleUserId { get; set; }

        public IEnumerable<string> NewOptions { get; set; }
    }
}
