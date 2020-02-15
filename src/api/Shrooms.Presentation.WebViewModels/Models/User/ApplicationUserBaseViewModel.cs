using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserBaseViewModel : IValidatableObject
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public virtual string Id { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}