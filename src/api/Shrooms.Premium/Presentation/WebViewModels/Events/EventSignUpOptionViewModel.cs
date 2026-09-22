using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventSignUpOptionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Carried so a client echoing this payload back does not reset a stored rule - and so it
        /// can enforce the rule while collecting answers. Both single-join checks now read every
        /// option, not just legacy ones, because a legacy option adopted into a question keeps its
        /// IgnoreSingleJoin: a client that drops this field offers combinations the join rejects.
        /// </summary>
        public OptionRules? Rule { get; set; }
    }
}
