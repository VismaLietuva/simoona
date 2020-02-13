using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.WebViewModels.Models.Projects
{
    public class NewProjectViewModel
    {
        [Required]
        [StringLength(WebApiConstants.ProjectNameMaxLength)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string OwningUserId { get; set; }

        public IEnumerable<string> MembersIds { get; set; }

        public IEnumerable<string> Attributes { get; set; }

        public string Logo { get; set; }
    }
}