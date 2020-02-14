using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.ServiceValidators.Validators.Kudos
{
    public interface IKudosServiceValidator
    {
        void ValidateUser(ApplicationUser user);
        void ValidateKudosType(KudosType kudosType);
        void ValidateKudosMinusPermission(bool hasPermission);
        void ValidateUserAvailableKudos(decimal userRemainingKudos, decimal totalKudosPoints);
        void ValidateUserAvailableKudosToSendPerMonth(decimal totalKudosPointsInLog, decimal kudosAvailableToSendThisMonth);
        void ValidateSendingToSameUserAsReceiving(string sendingUserId, string receivingUserId);
        void CheckForEmptyUserList(List<ApplicationUser> recievingUsers);
        void CheckIfUserExists(bool userExists);
    }
}
