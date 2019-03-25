using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.Jobs
{
    public class NewJobTypeViewModel
    {
        [Required]
        [StringLength(ConstWebApi.JobTitleMaxLength, MinimumLength = 1)]
        public string Title { get; set; }
    }
}
