namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts.Comments
{
    public class EditCommentDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
    }
}
