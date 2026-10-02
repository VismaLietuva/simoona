using System;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>
    /// Hangfire dashboard access. Job storage is shared by every tenant, the Admin role is assigned per
    /// tenant and user names are only unique within a tenant database, so the caller must be an Admin AND
    /// be listed in HangfireOperators as "organization:username" (semicolon-separated), which only
    /// deployment configuration can change.
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

            var organization = user.FindFirst(WebApiConstants.ClaimOrganizationName)?.Value;
            var userName = user.Identity.Name;
            if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(userName))
            {
                return false;
            }

            var configuration = httpContext.RequestServices.GetService<IConfiguration>();
            var operators = (configuration?[OperatorsSetting] ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var identity = organization + ":" + userName;
            return operators.Any(o => string.Equals(o, identity, StringComparison.OrdinalIgnoreCase));
        }
    }
}
