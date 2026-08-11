using Shrooms.Contracts.DataTransferObjects.Models.Emoji;

namespace Shrooms.Domain.Services.Emoji
{
    public class EmojiListCacheEntry
    {
        public CustomEmojiListDto List { get; set; }

        public long Generation { get; set; }
    }
}
