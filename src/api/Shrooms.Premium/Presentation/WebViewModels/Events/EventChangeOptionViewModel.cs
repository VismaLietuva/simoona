using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventChangeOptionViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question answers. Omit the property to keep the stored answers, send an empty array to
        /// clear them. Deliberately not [Required]: null and empty mean different things here.
        /// </summary>
        public IEnumerable<int> Answers { get; set; }
    }
}