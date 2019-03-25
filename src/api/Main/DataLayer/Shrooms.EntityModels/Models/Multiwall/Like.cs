using System;

namespace Shrooms.EntityModels.Models.Multiwall
{
    public class Like
    {
        public string UserId { get; private set; }

        public DateTime Created { get; private set; }

        public Like(string userId)
        {
            UserId = userId;
            Created = DateTime.UtcNow;
        }
    }
}
