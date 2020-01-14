using Shrooms.Constants.BusinessLayer.Events;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class NewEventOptionDTO
    {
        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
