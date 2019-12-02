using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Other.Shrooms.Constants.BusinessLayer;

namespace Shrooms.WebViewModels.Models.Events
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(ConstWebApi.EventNameMaxLength)]
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

        public List<int> Offices { get; set; }

        [Required]
        [StringLength(ConstWebApi.EventLocationMaxLength)]
        public string Location { get; set; }

        [StringLength(ConstWebApi.EventDescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        [Range(ConstWebApi.EventMinimumParticipants, ConstWebApi.EventMaxParticipants)]
        public int MaxParticipants { get; set; }

        [Range(ConstWebApi.EventMinimumOptions, short.MaxValue)]
        public int MaxOptions { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Range((int)EventConstants.FoodOptions.None, (int)EventConstants.FoodOptions.Optional)]
        public int? FoodOption { get; set; }

        [Required]
        public string ResponsibleUserId { get; set; }

        public IEnumerable<string> NewOptions { get; set; }
    }
}
