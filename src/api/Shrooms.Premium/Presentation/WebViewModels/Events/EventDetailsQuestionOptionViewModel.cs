using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Wire shape of a question option on GET /Events/Details: the wizard's option plus the people
    /// who picked it.
    /// </summary>
    public class EventDetailsQuestionOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public IEnumerable<EventDetailsParticipantViewModel> Participants { get; set; }
    }
}
