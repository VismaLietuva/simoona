using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class EditFilterPresetViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.FilterPresetMaxNameLength)]
        public string Name { get; set; }

        [Required]
        [EnumDataType(typeof(PageType))]
        public PageType PageType { get; set; }

        [Required]
        public bool IsDefault { get; set; }

        [Required]
        public IEnumerable<FilterPresetItemViewModel> Filters { get; set; }
    }
}
