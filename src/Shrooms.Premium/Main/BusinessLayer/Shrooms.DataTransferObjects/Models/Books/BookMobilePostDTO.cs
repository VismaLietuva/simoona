namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Books
{
    public class BookMobilePostDTO
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public int Quantity { get; set; }

        public string Code { get; set; }

        public int OfficeId { get; set; }

        public string ApplicationUserId { get; set; }

        public int OrganizationId { get; set; }
    }
}
