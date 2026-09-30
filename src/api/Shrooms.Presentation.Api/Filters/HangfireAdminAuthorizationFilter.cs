using Hangfire.Dashboard;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>
    /// Hangfire dashboard access: the request must carry an authenticated identity in the Admin role.
    /// Replaces the default LocalRequestsOnlyAuthorizationFilter, which cannot tell a proxy from a client.
    /// </summary>
    public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = context.GetHttpContext().User;
            return user?.Identity?.IsAuthenticated == true && user.IsInRole(Roles.Admin);
        }
    }
}
