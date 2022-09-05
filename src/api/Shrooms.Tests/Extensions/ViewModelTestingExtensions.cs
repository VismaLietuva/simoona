using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shrooms.Tests.Extensions
{
    public static class ViewModelTestingExtensions
    {
        public static bool IsValid<T>(this T viewModel) where T : class
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(viewModel, null, null);

            Validator.TryValidateObject(viewModel, validationContext, validationResults, true);

            return !validationResults.Any();
        }
    }
}
