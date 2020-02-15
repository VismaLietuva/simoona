namespace Shrooms.Premium.DataTransferObjects.Models.Books
{
    public class TakenBookDTO
    {
        public string UserId { get; set; }
        public int OrganizationId { get; set; }
        public int BookOfficeId { get; set; }
        public int OfficeId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}
