using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ServiceRequest : BaseModelWithOrg
    {
        public string EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public ApplicationUser Employee { get; set; }

        public string Title { get; set; }

        public int PriorityId { get; set; }

        [ForeignKey("PriorityId")]
        public ServiceRequestPriority Priority { get; set; }

        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public ServiceRequestStatus Status { get; set; }

        public string Description { get; set; }

        public string CategoryName { get; set; }

        public int? KudosAmmount { get; set; }

        public int? KudosShopItemId { get; set; }

        [ForeignKey("KudosShopItemId")]
        public KudosShopItem KudosShopItem { get; set; }

        public string PictureId { get; set; }
    }
}
