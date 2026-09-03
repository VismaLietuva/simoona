using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventQuestionOptionStructureDto
    {
        public int? Id { get; set; }

        public string ClientId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>Null means "leave as it is", so a client that omits it cannot reset a stored rule.</summary>
        public OptionRules? Rule { get; set; }
    }
}
