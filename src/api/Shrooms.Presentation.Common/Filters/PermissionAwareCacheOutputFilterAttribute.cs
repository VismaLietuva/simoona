using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Filters
{
    /// <summary>
    /// Caches an action's response per organization and per the subset of <see cref="Permissions"/>
    /// the caller actually holds. Endpoints decorated with this must return a body shaped only by
    /// those two things plus the query string, never by the caller's identity, because callers with
    /// matching permissions inside one organization share a cache entry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PermissionAwareCacheOutputFilterAttribute : Attribute, IOutputCachePolicy
    {
        private readonly string[] _permissions;

        /// <summary>How long a stored response stays valid, in seconds.</summary>
        public int ServerTimeSpan { get; set; }

        /// <summary>
        /// Eviction group. Writes that change what this endpoint returns evict this group for their
        /// own organization through <see cref="IWidgetCacheInvalidator"/>. Defaults to the
        /// controller name.
        /// </summary>
        public string CacheGroup { get; set; }

        public string[] Permissions => _permissions;

        public PermissionAwareCacheOutputFilterAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public async ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var httpContext = context.HttpContext;
            var tenant = httpContext.GetRequestedTenant();

            // Without a resolved tenant or an authenticated caller there is no safe key to store
            // under, so the request runs uncached rather than sharing someone else's entry.
            if (string.IsNullOrEmpty(tenant) || httpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.EnableOutputCaching = false;
                return;
            }

            context.EnableOutputCaching = true;
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;

            // Identical requests arrive in the same second when a wall loads, so let one of them
            // populate the entry while the rest wait instead of all querying the database.
            context.AllowLocking = true;

            context.ResponseExpirationTimeSpan = TimeSpan.FromSeconds(ServerTimeSpan);

            context.CacheVaryByRules.QueryKeys = "*";
            context.CacheVaryByRules.VaryByValues["tenant"] = tenant;
            context.CacheVaryByRules.VaryByValues["permissions"] = await BuildPermissionKeyAsync(httpContext);

            context.Tags.Add(WidgetCacheTag.For(ResolveCacheGroup(httpContext), tenant));
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var response = context.HttpContext.Response;

            // Only a plain successful body may be handed to the next caller. Anything carrying a
            // Set-Cookie is caller-specific by definition.
            if (response.StatusCode != StatusCodes.Status200OK || response.Headers.ContainsKey(HeaderNames.SetCookie))
            {
                context.AllowCacheStorage = false;
            }

            return ValueTask.CompletedTask;
        }

        private string ResolveCacheGroup(HttpContext httpContext)
        {
            if (!string.IsNullOrEmpty(CacheGroup))
            {
                return CacheGroup;
            }

            var actionDescriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            return actionDescriptor?.ControllerName ?? string.Empty;
        }

        private async Task<string> BuildPermissionKeyAsync(HttpContext httpContext)
        {
            var permissionService = httpContext.RequestServices.GetRequiredService<IPermissionService>();
            var userAndOrganization = httpContext.User.Identity.GetUserAndOrganization();

            // Declared order is preserved so the same permission set always produces the same key.
            var heldPermissions = new List<string>();

            foreach (var permission in _permissions)
            {
                if (await permissionService.UserHasPermissionAsync(userAndOrganization, permission))
                {
                    heldPermissions.Add(permission);
                }
            }

            return string.Join(",", heldPermissions);
        }
    }
}
