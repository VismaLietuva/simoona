using System;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.Presentation.WebViewModels.KudosShop;

namespace Shrooms.Premium.Presentation.WebViewModels.ServiceRequests
{
    // Returned as is by the API, and the client picks which navigations get loaded (includeProperties), so an entity-typed
    // member serializes whatever was loaded beneath it. Employee and KudosShopItem are view models for that reason: as
    // entities they carried the requester's password hash and the organization's settings. Priority and Status are
    // entities without navigations.
    public class ServiceRequestViewModel : AbstractViewModel
    {
        public DateTime Created { get; set; }

        public ServiceRequestEmployeeViewModel Employee { get; set; }

        public string Title { get; set; }

        public ServiceRequestPriority Priority { get; set; }

        public ServiceRequestStatus Status { get; set; }

        public string Description { get; set; }

        public ServiceRequestCategoryMiniViewModel ServiceRequestCategory { get; set; }

        public KudosShopItemViewModel KudosShopItem { get; set; }

        public string CategoryName { get; set; }

        public int? KudosAmmount { get; set; }

        public bool IsCloseable { get; set; }

        public string PictureId { get; set; }

        public bool IsKudosCommentHidden { get; set; }
    }
}
