using System;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>
    /// Hangfire dashboard access. Job storage is shared by every tenant and the Admin role is assigned per
    /// tenant, so the role alone is not enough: the caller must also be listed in HangfireOperators
    /// (semicolon-separated user names), which only deployment configuration can change.
    /// </summary>
    public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public const string OperatorsSetting = "HangfireOperators";

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated != true || !user.IsInRole(Roles.Admin))
            {
                return false;
            }

            var configuration = httpContext.RequestServices.GetService<IConfiguration>();
            var operators = (configuration?[OperatorsSetting] ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var userName = user.Identity.Name;
            return !string.IsNullOrEmpty(userName)
                && operators.Any(o => string.Equals(o, userName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
