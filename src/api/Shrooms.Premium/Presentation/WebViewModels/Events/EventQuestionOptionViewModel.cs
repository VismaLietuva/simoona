using System.ComponentModel.DataAnnotations;
using Hangfire.Annotations;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EventQuestionOptionViewModel
    {
        public int? Id { get; set; }

        public string ClientId { get; set; }

        [Required]
        [StringLength(EventsConstants.EventQuestionOptionNameMaxLength)]
        public string Name { get; set; }

        public int Order { get; set; }

        /// <summary>Omitting this leaves a stored rule as it is rather than resetting it.</summary>
        public OptionRules? Rule { get; set; }
    }
}
