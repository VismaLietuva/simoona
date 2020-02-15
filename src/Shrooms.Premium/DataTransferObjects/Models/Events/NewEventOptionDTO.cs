using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class NewEventOptionDTO
    {
        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
