namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments
{
    public class EditCommentDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
    }
}
