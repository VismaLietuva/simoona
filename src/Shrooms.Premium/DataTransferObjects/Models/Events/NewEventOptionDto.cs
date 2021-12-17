using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class NewEventOptionDto
    {
        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
