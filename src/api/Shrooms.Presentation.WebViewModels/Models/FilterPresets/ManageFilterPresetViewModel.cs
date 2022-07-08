using Shrooms.Contracts.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.FilterPresets
{
    public class ManageFilterPresetViewModel
    {
        [Required]
        [EnumDataType(typeof(PageType))]
        public PageType PageType { get; set; }
        
        public IEnumerable<UpdateFilterPresetViewModel> PresetsToUpdate;

        public IEnumerable<CreateFilterPresetViewModel> PresetsToCreate;

        public IEnumerable<int> PresetsToDelete;
    }
}
