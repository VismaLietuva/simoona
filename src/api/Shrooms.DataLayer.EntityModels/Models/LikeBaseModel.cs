using System.Linq;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public abstract class LikeBaseModel : BaseModel, ILikeable
    {
        public virtual LikesCollection Likes { get; set; }

        public bool IsLikedByUser(string userId)
        {
            return Likes.Any(a => a.UserId == userId);
        }
    }
}