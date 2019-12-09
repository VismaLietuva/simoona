using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Books
{
    public class BookReportDTO
    {
        public int BookOfficeId { get; set; }
        public string Report { get; set; }
        public string Comment { get; set; }
    }
}
