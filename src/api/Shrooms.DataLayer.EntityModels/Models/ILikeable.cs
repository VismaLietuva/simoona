using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public interface ILikeable
    {
        LikesCollection Likes { get; set; }
    }
}
