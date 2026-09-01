using Hangfire.Annotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EventQuestionConditionViewModel
    {
        /// <summary>Set when the trigger option already exists. Mutually exclusive with <see cref="OptionClientId"/>.</summary>
        public int? OptionId { get; set; }

        /// <summary>Set when the trigger option is inserted in this same request.</summary>
        public string OptionClientId { get; set; }
    }
}
