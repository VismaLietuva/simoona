namespace Shrooms.DomainServiceValidators.Validators.Wall
{
    public interface IWallValidator
    {
        void CheckIfUserIsWallMember(string userId, int? postWallId);
        void CheckIfUserCanCreatePostInWall(string userId, int? wallId);
    }
}
