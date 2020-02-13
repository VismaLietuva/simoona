using System.ComponentModel.DataAnnotations;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.WebViewModels.Models.Jobs
{
    public class NewJobTypeViewModel
    {
        [Required]
        [StringLength(WebApiConstants.JobTitleMaxLength, MinimumLength = 1)]
        public string Title { get; set; }
    }
}
