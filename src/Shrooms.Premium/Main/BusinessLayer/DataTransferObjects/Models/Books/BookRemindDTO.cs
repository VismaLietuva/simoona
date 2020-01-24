using System;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Books
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
