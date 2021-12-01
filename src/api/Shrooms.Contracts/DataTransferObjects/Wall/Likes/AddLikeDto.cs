using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Likes
{
    public class AddLikeDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [EnumDataType(typeof(LikeTypeEnum))]
        public LikeTypeEnum LikeType { get; set; }
    }
}