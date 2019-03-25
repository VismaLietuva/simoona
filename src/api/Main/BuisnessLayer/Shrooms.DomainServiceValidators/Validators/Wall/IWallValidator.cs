using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.EntityModels.Models;

namespace DomainServiceValidators.Validators.Wall
{
    public interface IWallValidator
    {
        void CheckIfUserIsWallMember(string userId, int? postWallId);
        void CheckIfUserCanCreatePostInWall(string userId, int? wallId);
    }
}
