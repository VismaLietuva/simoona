using Microsoft.AspNetCore.Http;

namespace Shrooms.Presentation.Common.Helpers
{
    public static class HttpContextHelpers
    {
        public static string GetRequestedTenant(this HttpContext context)
        {
            return context.Items["tenantName"] as string ?? string.Empty;
        }

        public static bool IsOrganizationValid(this HttpContext context)
        {
            return context.Items["isOrganizationValid"] is bool v && v;
        }
    }
}
