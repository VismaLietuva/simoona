using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventSignUpOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Carried on the read shape only so a client echoing this payload back does not reset a
        /// stored rule. It does not affect sign-up: both single-join checks are scoped to legacy
        /// options, so IgnoreSingleJoin on a question option is never consulted.
        /// </summary>
        public OptionRules? Rule { get; set; }
    }
}
