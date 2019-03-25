using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using Shrooms.API.Controllers;

namespace Shrooms.API.Handlers
{
    public class HttpNotFoundAwareControllerActionSelector : ApiControllerActionSelector
    {
        public HttpNotFoundAwareControllerActionSelector()
        {
        }

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            HttpActionDescriptor decriptor = null;
            try
            {
                decriptor = base.SelectAction(controllerContext);
            }
            catch (HttpResponseException ex)
            {
                var code = ex.Response.StatusCode;

                if (code != HttpStatusCode.NotFound && code != HttpStatusCode.MethodNotAllowed)
                {
                    throw;
                }

                IHttpController httpController = new ErrorController();
                controllerContext.Controller = httpController;
                controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "Error", httpController.GetType());
                controllerContext.RouteData.Values["action"] = "NotFound";
                controllerContext.RouteData.Values["path"] = controllerContext.Request.RequestUri.AbsoluteUri;

                decriptor = base.SelectAction(controllerContext);
            }

            return decriptor;
        }
    }
}