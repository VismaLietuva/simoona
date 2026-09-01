using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hangfire.Annotations;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EventQuestionViewModel
    {
        public int? Id { get; set; }

        /// <summary>Client-generated; required when <see cref="Id"/> is null.</summary>
        public string ClientId { get; set; }

        [Required]
        [StringLength(EventsConstants.EventQuestionTitleMaxLength)]
        public string Title { get; set; }

        public int Order { get; set; }

        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public EventQuestionConditionViewModel ShowIf { get; set; }

        /// <summary>
        /// Accepted so a client that echoes the read payload back keeps its conditions: the read
        /// shape carries the condition as this scalar, not as <see cref="ShowIf"/>. Used only when
        /// <see cref="ShowIf"/> is absent or carries no option of its own.
        /// </summary>
        public int? ShowIfOptionId { get; set; }

        public IList<EventQuestionOptionViewModel> Options { get; set; } = new List<EventQuestionOptionViewModel>();
    }
}
