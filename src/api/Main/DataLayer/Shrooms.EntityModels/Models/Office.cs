using System.Collections.Generic;
using Shrooms.EntityModels.Models.Books;

namespace Shrooms.EntityModels.Models
{
    public class Office : BaseModelWithOrg
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public virtual ICollection<BookOffice> BookOffices { get; set; }

        public virtual ICollection<Floor> Floors { get; set; }
    }
}