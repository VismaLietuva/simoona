using Shrooms.Premium.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventJoinViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        public AttendingStatus AttendStatus { get; set; }

        public string AttendComment { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question answers, kept apart from <see cref="ChosenOptions"/> so the flat food-style
        /// options and the question tree can be validated under their own rules.
        /// </summary>
        public IEnumerable<int> Answers { get; set; }
    }
}
