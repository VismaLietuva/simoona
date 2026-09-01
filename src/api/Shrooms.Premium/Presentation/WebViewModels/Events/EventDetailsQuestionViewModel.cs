using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Separate from <see cref="EventSignUpQuestionViewModel"/> because that one feeds a client
    /// component and must never carry participants.
    /// </summary>
    public class EventDetailsQuestionViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public int? ShowIfOptionId { get; set; }

        public IEnumerable<EventDetailsQuestionOptionViewModel> Options { get; set; }
    }
}
