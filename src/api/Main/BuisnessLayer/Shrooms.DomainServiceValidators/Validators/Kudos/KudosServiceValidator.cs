using System.Collections.Generic;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Exceptions;

namespace Shrooms.DomainServiceValidators.Validators.Kudos
{
    public class KudosServiceValidator : IKudosServiceValidator
    {
        public void ValidateUser(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }
        }

        public void ValidateKudosType(KudosType kudosType)
        {
            if (kudosType == null)
            {
                throw new ValidationException(ErrorCodes.KudosTypeNotFound, "Kudos type not found");
            }
        }

        public void ValidateKudosMinusPermission(bool hasPermission)
        {
            if (!hasPermission)
            {
                throw new UnauthorizedException();
            }
        }

        public void ValidateUserAvailableKudos(decimal userRemainingKudos, decimal totalKudosPoints)
        {
            if (userRemainingKudos < totalKudosPoints)
            {
                throw new ValidationException(ErrorCodes.InsufficientKudos, "Insufficient kudos points");
            }
        }

        public void ValidateUserAvailableKudosToSendPerMonth(decimal totalKudosPointsInLog, decimal kudosAvailableToSendThisMonth)
        {
            if (kudosAvailableToSendThisMonth < totalKudosPointsInLog)
            {
                throw new ValidationException(ErrorCodes.InsufficientKudos, "Insufficient kudos points");
            }
        }

        public void ValidateSendingToSameUserAsReceiving(string sendingUserId, string receivingUserId)
        {
            if (sendingUserId == receivingUserId)
            {
                throw new ValidationException(ErrorCodes.CanNotSendKudosToSelf, "Kudos receiver can not be a sender");
            }
        }

        public void CheckForEmptyUserList(List<ApplicationUser> recievingUsers)
        {
            if (recievingUsers.Count == 0)
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }
        }

        public void CheckIfUserExists(bool userExists)
        {
            if (!userExists)
            {
                throw new ValidationException(ErrorCodes.UserNotFound, "User not found");
            }
        }
    }
}
