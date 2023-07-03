using System.Security.Principal;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Presentation.Common.Helpers
{
    public static class IdentityExtensions
    {
        public static UserAndOrganizationDto GetUserAndOrganization(this IIdentity identity)
        {
            return new UserAndOrganizationDto
            {
                OrganizationId = identity.GetOrganizationId(),
                UserId = identity.GetUserId()
            };
        }
    }
}
