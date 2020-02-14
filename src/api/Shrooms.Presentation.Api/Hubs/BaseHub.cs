using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Presentation.Api.Hubs
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