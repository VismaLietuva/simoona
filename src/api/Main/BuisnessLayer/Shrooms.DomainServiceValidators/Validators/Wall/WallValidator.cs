using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.DAL;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Multiwall;
using static Shrooms.Constants.ErrorCodes.ErrorCodes;

namespace DomainServiceValidators.Validators.Wall
{
    public class WallValidator : IWallValidator
    {
        private readonly IDbSet<WallMember> _wallUsersDbSet;

        public WallValidator(IUnitOfWork2 uow)
        {
            _wallUsersDbSet = uow.GetDbSet<WallMember>();
        }

        public void CheckIfUserIsWallMember(string userId, int? postWallId)
        {
            ValidateWallAccessibility(userId, postWallId, CheckIfUserIsPartOfAnyWall);
        }

        public void CheckIfUserCanCreatePostInWall(string userId, int? postWallId)
        {
            ValidateWallAccessibility(userId, postWallId, CheckIfUserIsPartOfSubwall);
        }

        private void ValidateWallAccessibility(string userId, int? postWallId, Func<string, int, bool> HasUserAccessToWall)
        {
            //Check if post belongs to wall
            if (postWallId.HasValue)
            {
                if (!HasUserAccessToWall(userId, postWallId.Value))
                {
                    throw new ValidationException(UserIsNotAMemberOfWall, "Not permitted");
                }
            }
        }

        private bool CheckIfUserIsPartOfSubwall(string userId, int postWallId)
        {
            return _wallUsersDbSet
                                .Include(x => x.Wall)
                                .Any(x => x.WallId == postWallId &&
                                    x.UserId == userId);
        }

        private bool CheckIfUserIsPartOfAnyWall(string userId, int postWallId)
        {
            return _wallUsersDbSet
                                .Any(x => x.WallId == postWallId
                                    && x.UserId == userId);
        }
    }
}
