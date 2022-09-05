namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments
{
    public class MentionCommentDto
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public string AuthorFullName { get; set; }
    }
}
