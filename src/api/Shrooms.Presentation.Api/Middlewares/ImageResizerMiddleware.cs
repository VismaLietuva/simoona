using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Shrooms.Presentation.Api.Middlewares
{
    public class ImageResizerMiddleware : OwinMiddleware
    {
        public ImageResizerMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            if (request.User == null || request.User.Identity.IsAuthenticated == false)
            {
                var index = request.Path.Value.LastIndexOf('.');
                if (index > 0 && IsImage(request.Path.Value.Substring(index + 1)))
                {
                    Unauthorized(context);
                    return;
                }
            }

            try
            {
                await Next.Invoke(context);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static bool IsImage(string extension)
        {
            var validExtensions = new List<string> { "jpg", "jpeg", "bmp", "gif", "png", "tif", "tiff" };
            if (validExtensions.Contains(extension))
            {
                return true;
            }

            return false;
        }

        private static void Unauthorized(IOwinContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.ReasonPhrase = "Unauthorized";
        }
    }
}