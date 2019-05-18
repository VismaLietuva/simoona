using System.Security.Principal;
using Microsoft.AspNet.Identity;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.API.Helpers
{
    public static class IdentityExtensions
    {
        public static UserAndOrganizationDTO GetUserAndOrganization(this IIdentity identity)
        {
            return new UserAndOrganizationDTO
            {
                OrganizationId = identity.GetOrganizationId(),
                UserId = identity.GetUserId()
            };
        }
    }
}