using System;
using Shrooms.EntityModels.Models;
using Shrooms.WebViewModels.Models;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.ServiceRequests
{
    public class ServiceRequestViewModel : AbstractViewModel
    {
        public DateTime Created { get; set; }

        public ApplicationUser Employee { get; set; }

        public string Title { get; set; }

        public ServiceRequestPriority Priority { get; set; }

        public ServiceRequestStatus Status { get; set; }

        public string Description { get; set; }

        public ServiceRequestCategory ServiceRequestCategory { get; set; }

        public string CategoryName { get; set; }

        public int? KudosAmmount { get; set; }

        public bool IsCloseable { get; set; }

        public string PictureId { get; set; }
    }
}