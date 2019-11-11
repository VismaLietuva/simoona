using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Books
{
    public class BookRemindDTO
    {
        public string ApplicationUserId { get; set; }
        public int BookOfficeId { get; set; }
        public int OfficeId { get; set; }
        public DateTime TakenFrom { get; set; }
        public int OrganizationId { get; set; } 
        public string Title { get; set; }
        public string Author { get; set; }
    }
}
