using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails
{
    public class NewBookDto : UserAndOrganizationDto
    {
        public string Isbn { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string OwnerId { get; set; }

        public string Note { get; set; }

        public IEnumerable<NewBookQuantityDto> QuantityByOffice { get; set; }
    }
}
