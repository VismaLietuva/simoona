using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.Api.Middlewares
{
    public class MultiTenancyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public MultiTenancyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.ToString();

            if (requestPath.StartsWith("/signin-google", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/signin-facebook", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/signin-microsoft", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var tenantKey = ExtractTenant(context);

            if (string.IsNullOrEmpty(tenantKey))
            {
                context.Response.StatusCode = 401;
                return;
            }

            if (!TryFindTenant(out var tenantName, tenantKey))
            {
                await ReturnInvalidOrganizationResponseAsync(context);
                return;
            }

            context.Items["tenantName"] = tenantName;

            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static string ExtractTenant(HttpContext context)
        {
            var tenantKey = default(string);
            var requestPath = context.Request.Path.ToString();

            if (requestPath.StartsWith("/storage", StringComparison.OrdinalIgnoreCase))
            {
                var parts = requestPath.Split('/');
                tenantKey = parts.Length > 2 ? parts[2] : null;
            }
            else if (context.User != null &&
                context.User.Identity.IsAuthenticated &&
                context.User.Claims.Any(x => x.Type == "OrganizationName"))
            {
                tenantKey = context.User.Claims.First(x => x.Type == "OrganizationName").Value.ToLowerInvariant();
            }
            else if (requestPath.StartsWith("/token", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/externaljobs", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/externalpremiumjobs", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/ExternalLogin", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/RegisterExternal", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/InternalLogins", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/UserInfo", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/Register", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/ResetPassword", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/RequestPasswordReset", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/Account/VerifyEmail", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWith("/bookmobile", StringComparison.OrdinalIgnoreCase))
            {
                var organizationFromHeader = context.Request.Headers["Organization"].FirstOrDefault();
                var organizationFromUri = context.Request.Query["organization"].FirstOrDefault();

                tenantKey = organizationFromHeader ?? organizationFromUri;
            }

            return tenantKey;
        }

        private static async Task ReturnInvalidOrganizationResponseAsync(HttpContext context)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var responseBody = new
            {
                errorCode = ErrorCodes.InvalidOrganization,
                errorMessage = "Invalid organization"
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(responseBody));
        }

        private bool TryFindTenant(out string tenantName, string tenantKey)
        {
            // Try Organizations section in configuration
            tenantName = _configuration[$"Organizations:{tenantKey}"] != null ? tenantKey : null;
            if (tenantName != null)
            {
                return true;
            }

            // Fall back to checking if a connection string exists with this name
            var connStr = _configuration.GetConnectionString(tenantKey);
            if (!string.IsNullOrEmpty(connStr))
            {
                tenantName = tenantKey;
                return true;
            }

            tenantName = null;
            return false;
        }
    }
}