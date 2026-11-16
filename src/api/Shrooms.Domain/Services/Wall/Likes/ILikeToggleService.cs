using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Wall.Likes
{
    public interface ILikeToggleService
    {
        Task ToggleAsync(LikesCollection likes, AddLikeDto addLikeDto, UserAndOrganizationDto userOrg);
    }
}
