using System.Collections.Generic;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class EventDetailsOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<EventDetailsParticipantViewModel> Participants { get; set; }
    }
}
