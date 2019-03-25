using System;

namespace Shrooms.DataTransferObjects.Models.Books
{
    public class BookLogDTO
    {
        public int Id { get; set; }

        public DateTime? TakenFrom { get; set; }

        public DateTime? TakenTill { get; set; }

        public string ApplicationUserId { get; set; }

        public string ApplicationUserFirstName { get; set; }

        public string ApplicationUserLastName { get; set; }

        public int BookId { get; set; }

        public int OrganizationId { get; set; }

        public int OfficeId { get; set; }

        public string BookTakerId { get; set; }
    }
}
