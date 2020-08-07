using Shrooms.Contracts.DataTransferObjects.Users;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts
{
    public class EditPostDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
    }
}
