using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Read shape for the attendee wizard. Deliberately separate from
    /// <see cref="EventQuestionViewModel"/>: that one is the write shape, carrying clientId and a
    /// nested ShowIf object that mean nothing on read. Keeping them apart also lets SelectType go
    /// over the wire as a string — no global string-enum converter is configured, so reusing the
    /// write model would emit 0/1 and break the client.
    /// </summary>
    public class EventSignUpQuestionViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public int? ShowIfOptionId { get; set; }

        public IList<EventSignUpOptionViewModel> Options { get; set; } = new List<EventSignUpOptionViewModel>();
    }
}
