using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestUpdateViewModel : ServiceRequestCreateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int StatusId { get; set; }
    }
}
