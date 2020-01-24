namespace Shrooms.DataTransferObjects.Models.Wall.Posts.Comments
{
    public class CommentCreatedDTO
    {
        public int WallId { get; set; }

        public int PostId { get; set; }

        public int CommentId { get; set; }

        public WallType WallType { get; set; }

        public string PostCreator { get; set; }

        public string CommentCreator { get; set; }
    }
}
