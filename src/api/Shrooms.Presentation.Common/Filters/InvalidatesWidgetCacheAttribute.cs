using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Shrooms.Presentation.Common.Filters
{
    /// <summary>
    /// Evicts a widget cache group for the caller's organization once the action has succeeded.
    /// Replaces WebApi.OutputCache.V2's [InvalidateCacheOutput], which the .NET 10 migration
    /// dropped along with the caching it depended on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InvalidatesWidgetCacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _cacheGroup;

        public InvalidatesWidgetCacheAttribute(string cacheGroup)
        {
            _cacheGroup = cacheGroup;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            // A failed write changed nothing, so the cached response is still accurate.
            if (executed.Exception != null && !executed.ExceptionHandled)
            {
                return;
            }

            var invalidator = context.HttpContext.RequestServices.GetRequiredService<IWidgetCacheInvalidator>();
            await invalidator.InvalidateAsync(_cacheGroup, context.HttpContext.RequestAborted);
        }
    }
}
