using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class AbstractClassifierAbstractViewModel : AbstractViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Value { get; set; }

        public virtual string SortOrder { get; set; }

        public string AbstractClassifierType { get; set; }

        public string OrganizationName { get; set; }

        public string ParentName { get; set; }

        public int? ParentId { get; set; }

        public int? OrganizationId { get; set; }

        public virtual AbstractClassifierViewModel Parent { get; set; }

        public IEnumerable<AbstractClassifierViewModel> Children { get; set; }
    }
}