using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.ViewModels.Wall.Likes
{
    public class AddLikeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [EnumDataType(typeof(LikeTypeEnum))]
        public LikeTypeEnum LikeType { get; set; }
    }
}