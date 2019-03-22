using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestUpdateViewModel : ServiceRequestCreateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int StatusId { get; set; }
    }
}
