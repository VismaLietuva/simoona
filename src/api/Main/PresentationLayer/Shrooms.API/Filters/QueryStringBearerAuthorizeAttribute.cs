using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;

namespace Shrooms.API.Filters
{
    public class QueryStringBearerAuthorizeAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            return request.User?.Identity != null && request.User.Identity.IsAuthenticated;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;
            var principal = hubIncomingInvokerContext.Hub.Context.Request.User;
            var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;

            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return false;
            }

            hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);
            return true;
        }
    }
}