using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class AddEditDeleteFilterPresetViewModel
    {
        [Required]
        [EnumDataType(typeof(PageType))]
        public PageType PageType { get; set; }
        
        public IEnumerable<EditFilterPresetViewModel> PresetsToUpdate;

        public IEnumerable<CreateFilterPresetViewModel> PresetsToAdd;

        public IEnumerable<int> PresetsToRemove;
    }
}
