using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventSignUpOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Carried so a client echoing this payload back does not reset a stored rule, and can enforce it.
        /// </summary>
        public OptionRules? Rule { get; set; }
    }
}
