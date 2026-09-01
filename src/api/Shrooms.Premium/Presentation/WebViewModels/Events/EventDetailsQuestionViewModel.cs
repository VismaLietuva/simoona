using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Read shape for the host's responses panel. Mirrors <see cref="EventSignUpQuestionViewModel"/>
    /// field for field, adding the participants behind each option — which is exactly why it is a
    /// separate model: the wizard's payload is serialised into a client component and must never
    /// carry names. SelectType needs the same explicit string converter; no global one is configured.
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
