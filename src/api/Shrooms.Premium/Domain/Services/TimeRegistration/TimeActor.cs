using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeActor
    {
        public TimeActor(UserAndOrganizationDto viewer, string userId, bool isAccountant)
        {
            Viewer = viewer;
            UserId = userId;
            IsAccountant = isAccountant;
        }

        public UserAndOrganizationDto Viewer { get; }

        public string UserId { get; }

        public bool IsAccountant { get; }

        public bool IsSelf => TimeRegistrationRules.SameUser(UserId, Viewer.UserId);

        public int OrganizationId => Viewer.OrganizationId;
    }
}
