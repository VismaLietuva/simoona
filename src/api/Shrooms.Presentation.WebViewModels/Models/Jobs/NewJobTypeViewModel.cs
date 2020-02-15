using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Jobs
{
    public class NewJobTypeViewModel
    {
        [Required]
        [StringLength(WebApiConstants.JobTitleMaxLength, MinimumLength = 1)]
        public string Title { get; set; }
    }
}
