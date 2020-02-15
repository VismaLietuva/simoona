using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Contracts.ViewModels
{
    public class AbstractViewModel : IValidatableObject
    {
        public int Id { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}