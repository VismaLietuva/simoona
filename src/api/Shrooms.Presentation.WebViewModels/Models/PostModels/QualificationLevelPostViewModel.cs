using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class QualificationLevelPostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        public IEnumerable<AbstractViewModel> ApplicationUsers { get; set; }

        public int SortOrder { get; set; }
    }
}