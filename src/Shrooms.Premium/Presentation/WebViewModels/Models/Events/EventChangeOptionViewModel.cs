using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Events
{
    public class EventChangeOptionViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public IEnumerable<int> ChosenOptions { get; set; }
    }
}