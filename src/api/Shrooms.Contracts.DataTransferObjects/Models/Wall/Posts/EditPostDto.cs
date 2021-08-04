namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts
{
    public class EditPostDto : UserAndOrganizationDto
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
    }
}
