using Shrooms.Premium.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventJoinMultipleViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        public AttendingStatus AttendStatus { get; set; }

        [Required]
        public ICollection<string> ParticipantIds { get; set; }

        public ICollection<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question answers, applied to every participant being added.
        /// </summary>
        public ICollection<int> Answers { get; set; }
    }
}