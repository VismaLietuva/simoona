using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Books
{
    public class BookMobileDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public string Code { get; set; }

        public int OfficeId { get; set; }

        public string ApplicationUserId { get; set; }

        public int OrganizationId { get; set; }

        public List<OfficeBookDto> Offices { get; set; }
    }
}
