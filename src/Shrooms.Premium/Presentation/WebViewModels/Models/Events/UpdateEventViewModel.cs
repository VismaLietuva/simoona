using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Events
{
    public class UpdateEventViewModel : CreateEventViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public bool ResetParticipantList { get; set; }

        public IEnumerable<EventOptionViewModel> EditedOptions { get; set; }
    }
}
