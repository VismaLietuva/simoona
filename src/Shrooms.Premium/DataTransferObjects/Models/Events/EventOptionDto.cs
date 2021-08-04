using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventOptionDto
    {
        public int Id { get; set; }

        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
