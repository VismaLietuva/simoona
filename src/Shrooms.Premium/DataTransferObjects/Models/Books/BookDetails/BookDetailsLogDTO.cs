using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Books.BookDetails
{
    public class BookDetailsLogDTO
    {
        public string UserId { get; set; }
        public int LogId { get; set; }
        public DateTime TakenFrom { get; set; }
        public DateTime TakenTill { get; set; }
        public string FullName { get; set; }
        public DateTime? Returned { get; set; }
    }
}
