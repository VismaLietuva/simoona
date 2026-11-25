using System;
using Newtonsoft.Json;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class Like
    {
        public string UserId { get; private set; }

        public DateTime Created { get; private set; }

        public LikeTypeEnum Type { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EmojiName { get; private set; }

        public Like(string userId, LikeTypeEnum type, string emojiName = null)
        {
            UserId = userId;
            Created = DateTime.UtcNow;
            Type = type;
            EmojiName = emojiName;
        }
    }
}