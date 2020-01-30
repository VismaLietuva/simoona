using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Events
{
    public class EventChangeOptionViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public IEnumerable<int> ChosenOptions { get; set; }
    }
}