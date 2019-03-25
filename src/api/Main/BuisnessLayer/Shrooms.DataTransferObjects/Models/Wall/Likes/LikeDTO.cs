using Shrooms.DataTransferObjects.Models.Users;

namespace Shrooms.DataTransferObjects.Models.Wall.Likes
{
    public class LikeDto
    {
        public int Id { get; set; }
        public string LikeUserId { get; set; }
        public UserDto User { get; set; }
    }
}
