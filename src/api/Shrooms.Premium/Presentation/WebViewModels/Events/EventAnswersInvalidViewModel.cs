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

        /// <summary>
        /// The same value as <see cref="Code"/>. Every other EventException returns the bare code as
        /// the response body, so a client that reads a 400 as a string maps it straight through.
        /// This endpoint returns an object instead, and the existing error handler resolves an
        /// object body via its "message" property: without this it reaches none of its branches and
        /// reports an empty error. New clients should read <see cref="Errors"/>.
        /// </summary>
        public string Message { get; set; }

        public IList<EventAnswerErrorViewModel> Errors { get; set; } = new List<EventAnswerErrorViewModel>();
    }
}
