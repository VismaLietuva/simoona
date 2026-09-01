using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventQuestionViewModel
    {
        public int? Id { get; set; }

        /// <summary>Client-generated; required when <see cref="Id"/> is null.</summary>
        public string ClientId { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public EventQuestionConditionViewModel ShowIf { get; set; }

        /// <summary>
        /// Accepted so a client that echoes the read payload back keeps its conditions: the read
        /// shape carries the condition as this scalar, not as <see cref="ShowIf"/>. Ignored when
        /// <see cref="ShowIf"/> is set.
        /// </summary>
        public int? ShowIfOptionId { get; set; }

        public IList<EventQuestionOptionViewModel> Options { get; set; } = new List<EventQuestionOptionViewModel>();
    }

    public class EventQuestionConditionViewModel
    {
        /// <summary>Set when the trigger option already exists. Mutually exclusive with <see cref="OptionClientId"/>.</summary>
        public int? OptionId { get; set; }

        /// <summary>Set when the trigger option is inserted in this same request.</summary>
        public string OptionClientId { get; set; }
    }
}
