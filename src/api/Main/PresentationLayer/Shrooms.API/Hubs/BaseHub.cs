using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Shrooms.API.Helpers;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.API.Hubs
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