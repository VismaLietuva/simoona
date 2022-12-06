using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class UpdateEventViewModel : CreateEventViewModel
    {
        [Required]
        public Guid Id { get; set; }

        public bool ResetParticipantList { get; set; }

        public IEnumerable<EventOptionViewModel> EditedOptions { get; set; }
    }
}
