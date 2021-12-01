using System;
using Newtonsoft.Json;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class Like
    {
        public string UserId { get; private set; }

        public DateTime Created { get; private set; }

        [JsonProperty("Type")]
        public LikeTypeEnum Type { get; private set; }
        
        public Like(string userId, LikeTypeEnum likeType)
        {
            UserId = userId;
            Created = DateTime.UtcNow;
            Type = likeType;
        }
    }
}