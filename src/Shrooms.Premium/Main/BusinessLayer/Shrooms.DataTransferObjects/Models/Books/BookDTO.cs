using System;

namespace Shrooms.DataTransferObjects.Models.Books
{
    public class BookDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public int Quantity { get; set; }

        public string Code { get; set; }

        public int OfficeId { get; set; }

        public string OfficeName { get; set; }

        public int BookLogId { get; set; }

        public DateTime? TakenFrom { get; set; }

        public DateTime? TakenTill { get; set; }

        public string ApplicationUserId { get; set; }

        public string ApplicationUserFirstName { get; set; }

        public string ApplicationUserLastName { get; set; }
    }
}
