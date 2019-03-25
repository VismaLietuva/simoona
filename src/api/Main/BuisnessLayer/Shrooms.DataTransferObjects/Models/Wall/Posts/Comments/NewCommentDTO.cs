namespace Shrooms.DataTransferObjects.Models.Wall.Posts.Comments
{
    public class NewCommentDTO : UserAndOrganizationDTO
    {
        public int PostId { get; set; }
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
    }
}
