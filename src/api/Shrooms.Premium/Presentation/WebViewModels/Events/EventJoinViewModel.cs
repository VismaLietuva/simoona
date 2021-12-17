using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventJoinViewModel
    {
        [Required]
        public Guid EventId { get; set; }

        public int AttendStatus { get; set; }

        public string AttendComment { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }
    }
}
