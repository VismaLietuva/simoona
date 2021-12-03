using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Likes
{
    public class AddLikeDto
    {
        public int Id { get; set; }

        public LikeTypeEnum Type { get; set; }
    }
}