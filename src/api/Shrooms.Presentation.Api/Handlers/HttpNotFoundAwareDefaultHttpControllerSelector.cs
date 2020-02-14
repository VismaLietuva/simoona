using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Shrooms.Presentation.Api.Handlers
{
    public class HttpNotFoundAwareDefaultHttpControllerSelector : DefaultHttpControllerSelector
    {
        public HttpNotFoundAwareDefaultHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            HttpControllerDescriptor decriptor = null;
            try
            {
                decriptor = base.SelectController(request);
            }
            catch (HttpResponseException ex)
            {
                var code = ex.Response.StatusCode;
                if (code != HttpStatusCode.NotFound)
                {
                    throw;
                }

                var routeValues = request.GetRouteData().Values;
                routeValues["controller"] = "Error";
                routeValues["action"] = "NotFound";
                routeValues["path"] = request.RequestUri.AbsoluteUri;

                decriptor = base.SelectController(request);
            }

            return decriptor;
        }
    }
}