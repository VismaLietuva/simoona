using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventAnswerErrorViewModel
    {
        /// <summary>Null when the failure has no owning question, as for an unknown option.</summary>
        public int? QuestionId { get; set; }

        /// <summary>Set when the failure names a specific option, as for an unknown option.</summary>
        public int? OptionId { get; set; }

        /// <summary>
        /// Serialised as a string: no global string-enum converter is configured, and the wizard
        /// branches on this value.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventAnswerErrorReason Reason { get; set; }
    }
}
