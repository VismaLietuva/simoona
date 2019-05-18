using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestCommentPostViewModel
    {
        [Required]
        public int ServiceRequestId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}