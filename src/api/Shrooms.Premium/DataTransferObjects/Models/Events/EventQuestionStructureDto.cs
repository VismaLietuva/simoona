using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventQuestionStructureDto
    {
        public int? Id { get; set; }

        /// <summary>Client-generated, required when <see cref="Id"/> is null.</summary>
        public string ClientId { get; set; }

        public string Title { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }

        /// <summary>Set when the trigger option already exists in the database.</summary>
        public int? ShowIfOptionId { get; set; }

        /// <summary>Set when the trigger option is being inserted in this same request.</summary>
        public string ShowIfOptionClientId { get; set; }

        public IList<EventQuestionOptionStructureDto> Options { get; set; } = new List<EventQuestionOptionStructureDto>();
    }

    public class EventQuestionOptionStructureDto
    {
        public int? Id { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public OptionRules Rule { get; set; }
    }
}
