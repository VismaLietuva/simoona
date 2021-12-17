namespace Shrooms.Premium.DataTransferObjects.Models.Books
{
    public class BookMobilePostDto
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
