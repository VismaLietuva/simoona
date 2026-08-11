using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Emoji
{
    public class CustomEmojiListDto
    {
        public IEnumerable<CustomEmojiDto> Emojis { get; set; }

        public string ETag { get; set; }
    }
}
