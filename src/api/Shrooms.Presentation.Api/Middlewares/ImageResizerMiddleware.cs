using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Shrooms.Presentation.Api.Middlewares
{
    public class ImageResizerMiddleware
    {
        private readonly RequestDelegate _next;

        public ImageResizerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User == null || context.User.Identity.IsAuthenticated == false)
            {
                var pathValue = context.Request.Path.Value ?? string.Empty;
                var index = pathValue.LastIndexOf('.');
                if (index > 0 && IsImage(pathValue.Substring(index + 1)) && !pathValue.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static bool IsImage(string extension)
        {
            var validExtensions = new List<string> { "jpg", "jpeg", "bmp", "gif", "png", "tif", "tiff" };
            return validExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}