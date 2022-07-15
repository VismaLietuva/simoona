using Shrooms.Contracts.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class CreateFilterPresetViewModel
    {
        [Required]
        [StringLength(ValidationConstants.FilterPresetMaxNameLength)]
        public string Name { get; set; }

        [Required]
        public bool IsDefault { get; set; }

        [Required]
        public IEnumerable<FilterPresetItemViewModel> Filters { get; set; }
    }
}
