namespace Shrooms.Premium.DataTransferObjects.Models.Books
{
    public class RetrievedBookInfoDto
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public int BookOfficeId { get; set; }
        public string OwnerId { get; set; }
        public string Note { get; set; }
    }
}
