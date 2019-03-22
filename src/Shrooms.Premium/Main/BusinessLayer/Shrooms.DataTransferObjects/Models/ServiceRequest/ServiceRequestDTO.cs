namespace Shrooms.DataTransferObjects.Models.ServiceRequest
{
    public class ServiceRequestDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int PriorityId { get; set; }

        public string Description { get; set; }

        public int ServiceRequestCategoryId { get; set; }

        public int? KudosAmmount { get; set; }

        public int StatusId { get; set; }

        public string CategoryName { get; set; }

        public int? KudosShopItemId { get; set; }

        public string PictureId { get; set; }
    }
}
