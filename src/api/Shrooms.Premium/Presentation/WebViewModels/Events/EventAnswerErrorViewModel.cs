using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventAnswerErrorViewModel
    {
        /// <summary>The question the wizard should open on.</summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Serialised as a string: no global string-enum converter is configured, and the wizard
        /// branches on this value.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventAnswerErrorReason Reason { get; set; }
    }
}
