using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Likes
{
    public class LikeDto
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string PictureId { get; set; }

        public LikeTypeEnum Type { get; set; }
    }
}