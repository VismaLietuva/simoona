using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Models.Events
{
    public class EventDetailsOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<EventDetailsParticipantViewModel> Participants { get; set; }
    }
}
