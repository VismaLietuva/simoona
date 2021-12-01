using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.ViewModels.Wall.Likes
{
    public class LikeViewModel
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string PictureId { get; set; }

        public LikeTypeEnum LikeType { get; set; }
    }
}