using System;
using Shrooms.DataTransferObjects.Models.Users;
using Shrooms.Host.Contracts.Enums;

namespace Shrooms.DataTransferObjects.Models.Wall.Posts
{
    public class NewlyCreatedPostDTO
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public string PictureId { get; set; }

        public UserDto User { get; set; }

        public WallType WallType { get; set; }
        public int WallId { get; set; }
    }
}
