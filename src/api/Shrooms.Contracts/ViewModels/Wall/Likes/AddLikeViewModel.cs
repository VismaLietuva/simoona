using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.ViewModels.Wall.Likes
{
    public class AddLikeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [EnumDataType(typeof(LikeTypeEnum))]
        public LikeTypeEnum Type { get; set; }

        [StringLength(ValidationConstants.ReactionEmojiNameMaxLength)]
        public string EmojiName { get; set; }
    }
}