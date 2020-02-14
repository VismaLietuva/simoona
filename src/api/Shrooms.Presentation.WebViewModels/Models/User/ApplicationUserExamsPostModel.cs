using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserExamsPostModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public IEnumerable<int> ExamIds { get; set; }
    }
}