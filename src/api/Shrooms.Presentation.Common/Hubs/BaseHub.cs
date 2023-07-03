using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Hubs
{
    public abstract class BaseHub : Hub
    {
        protected UserAndOrganizationHubDto GetUserAndTenant()
        {
            var userHub = new UserAndOrganizationHubDto
            {
                UserId = Context.User.Identity.GetUserId(),
                OrganizationName = Context.User.Identity.GetOrganizationName(),
                OrganizationId = Context.User.Identity.GetOrganizationId()
            };
            return userHub;
        }
    }
}
