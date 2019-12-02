using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.EntityModels.Models.Books;

namespace Shrooms.EntityModels.Models
{
    public class Book : BaseModelWithOrg
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string Code { get; set; }

        public virtual ICollection<BookOffice> BookOffices { get; set; }

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        
        public string Note { get; set; }

        public string BookCoverUrl { get; set; }
    }
}
