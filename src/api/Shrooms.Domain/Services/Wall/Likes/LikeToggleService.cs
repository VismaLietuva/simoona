using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Helpers;

namespace Shrooms.Domain.Services.Wall.Likes
{
    public class LikeToggleService : ILikeToggleService
    {
        private static readonly Regex EmojiNameRegex = new(ValidationConstants.ReactionEmojiNameRegexPattern, RegexOptions.Compiled);

        private readonly DbSet<CustomEmoji> _customEmojisDbSet;

        public LikeToggleService(IUnitOfWork2 uow)
        {
            _customEmojisDbSet = uow.GetDbSet<CustomEmoji>();
        }

        public async Task ToggleAsync(LikesCollection likes, AddLikeDto addLikeDto, UserAndOrganizationDto userOrg)
        {
            CheckEmojiNameMatchesType(addLikeDto);

            var existing = likes.FirstOrDefault(like =>
                like.UserId == userOrg.UserId &&
                like.Type == addLikeDto.Type &&
                like.EmojiName == addLikeDto.EmojiName);

            if (existing != null)
            {
                likes.Remove(existing);
                return;
            }

            if (addLikeDto.Type == LikeTypeEnum.Emoji)
            {
                await CheckEmojiExistsAsync(addLikeDto.EmojiName, userOrg.OrganizationId);
            }

            likes.Add(new Like(userOrg.UserId, addLikeDto.Type, addLikeDto.EmojiName));
        }

        private static void CheckEmojiNameMatchesType(AddLikeDto addLikeDto)
        {
            var isValid = addLikeDto.Type == LikeTypeEnum.Emoji
                ? addLikeDto.EmojiName != null && EmojiNameRegex.IsMatch(addLikeDto.EmojiName)
                : addLikeDto.EmojiName == null;

            if (!isValid)
            {
                throw InvalidEmoji();
            }
        }

        private async Task CheckEmojiExistsAsync(string emojiName, int organizationId)
        {
            if (StandardEmojiTable.TryGet(emojiName, out _))
            {
                return;
            }

            if (!await _customEmojisDbSet.AnyAsync(emoji => emoji.OrganizationId == organizationId && emoji.Name == emojiName))
            {
                throw InvalidEmoji();
            }
        }

        private static ValidationException InvalidEmoji()
        {
            return new ValidationException(ErrorCodes.ReactionEmojiInvalid, "Reaction emoji is invalid");
        }
    }
}
