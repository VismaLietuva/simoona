﻿using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books.BookDetails
{
    public class NewBookDTO : UserAndOrganizationDTO
    {
        public string Isbn { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string OwnerId { get; set; }

        public string Note { get; set; }

        public IEnumerable<NewBookQuantityDTO> QuantityByOffice { get; set; }
    }
}
