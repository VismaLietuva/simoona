using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.Jobs
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
