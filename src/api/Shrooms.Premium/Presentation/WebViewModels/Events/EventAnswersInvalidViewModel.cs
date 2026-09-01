using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    /// <summary>
    /// Machine-readable answer failures, so the wizard can open on the offending step instead of
    /// showing a generic error.
    /// </summary>
    public class EventAnswersInvalidViewModel
    {
        public string Code { get; set; }

        public IList<EventAnswerErrorViewModel> Errors { get; set; } = new List<EventAnswerErrorViewModel>();
    }
}
