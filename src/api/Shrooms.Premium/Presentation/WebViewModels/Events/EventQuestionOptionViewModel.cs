using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventQuestionOptionViewModel
    {
        public int? Id { get; set; }

        public string ClientId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public OptionRules Rule { get; set; }
    }
}
