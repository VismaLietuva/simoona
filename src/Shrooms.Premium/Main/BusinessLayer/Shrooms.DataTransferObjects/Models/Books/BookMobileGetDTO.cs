using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Books
{
    public class BookMobileGetDTO
    {
        public string Code { get; set; }

        public int OrganizationId { get; set; }

        public int OfficeId { get; set; }
    }
}
