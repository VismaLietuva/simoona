using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventSignUpOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Carried on the read shape so a client echoing this payload back does not reset a
        /// stored rule. Rule on a question option feeds the single-join exemption checks, the
        /// same as it does for a legacy option.
        /// </summary>
        public OptionRules? Rule { get; set; }
    }
}
