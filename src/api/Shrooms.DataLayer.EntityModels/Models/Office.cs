using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models.Books;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Office : SoftDeletableModelWithOrg
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public virtual ICollection<BookOffice> BookOffices { get; set; }

        public virtual ICollection<Floor> Floors { get; set; }
    }
}