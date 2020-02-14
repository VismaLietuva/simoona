using System.Security.Principal;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Presentation.Api.Helpers
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