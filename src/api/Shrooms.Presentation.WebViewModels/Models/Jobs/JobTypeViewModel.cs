using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Jobs
{
    public class JobTypeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(WebApiConstants.JobTitleMaxLength, MinimumLength = 1)]
        public string Title { get; set; }
    }
}
