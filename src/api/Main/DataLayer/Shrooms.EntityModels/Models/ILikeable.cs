using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.EntityModels.Models
{
    public interface ILikeable
    {
        LikesCollection Likes { get; set; }
    }
}
