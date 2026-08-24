using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models.Support
{
    public class SupportPostViewModel : AbstractViewModel
    {
        [Required]
        [StringLength(ValidationConstants.SupportSubjectMaxLength)]
        public string Subject { get; set; }

        [Required]
        [StringLength(ValidationConstants.SupportMessageBodyMaxLength)]
        public string Message { get; set; }

        [Required]
        public int Type { get; set; }

        /// <summary>
        /// Optional screenshots, at most WebApiConstants.MaximumSupportImageCount of
        /// them. Submitted as multipart/form-data, so this model no longer binds from
        /// a JSON body.
        /// </summary>
        public List<IFormFile> Images { get; set; }
    }
}