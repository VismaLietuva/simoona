using Shrooms.Constants.BusinessLayer.Events;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class NewEventOptionDTO
    {
        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
