using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Books.BooksByOffice
{
    public class BooksByOfficeOptionsDto : UserAndOrganizationDto
    {
        public int OfficeId { get; set; }
        public string SearchString { get; set; }
        public int Page { get; set; }
    }
}
