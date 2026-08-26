using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Machine-readable answer failures, so the wizard can open on the offending step instead of
    /// showing a generic error. Reasons go over the wire as strings for the same reason
    /// <see cref="EventSignUpQuestionViewModel.SelectType"/> does.
    /// </summary>
    public class EventAnswersInvalidViewModel
    {
        public string Code { get; set; }

        public IList<EventAnswerErrorViewModel> Errors { get; set; } = new List<EventAnswerErrorViewModel>();
    }

    public class EventAnswerErrorViewModel
    {
        /// <summary>Null for UnknownOption, which has no owning question.</summary>
        public int? QuestionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventAnswerErrorReason Reason { get; set; }
    }
}
