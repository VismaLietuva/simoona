using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models.Books
{
    public class BookLog : BaseModelWithOrg
    {
        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public DateTime TakenFrom { get; set; }

        public DateTime? Returned { get; set; }

        public int BookOfficeId { get; set; }

        public BookOffice BookOffice { get; set; }
    }
}
