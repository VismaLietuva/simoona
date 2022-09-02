using System;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
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

        public KudosShopItem KudosShopItem { get; set; }

        public string CategoryName { get; set; }

        public int? KudosAmmount { get; set; }

        public bool IsCloseable { get; set; }

        public string PictureId { get; set; }
    }
}