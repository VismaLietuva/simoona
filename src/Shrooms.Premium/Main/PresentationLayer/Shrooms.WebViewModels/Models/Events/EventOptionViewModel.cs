
using Shrooms.Constants.BusinessLayer.Events;

namespace Shrooms.WebViewModels.Models.Events
{
    public class EventOptionViewModel
    {
        public int Id { get; set; }

        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}