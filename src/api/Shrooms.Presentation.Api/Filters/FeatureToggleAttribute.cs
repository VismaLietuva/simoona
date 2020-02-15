using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ReallySimpleFeatureToggle;
using Shrooms.Infrastructure.FeatureToggle;

namespace Shrooms.Presentation.Api.Filters
{
    public class FeatureToggleAttribute : ActionFilterAttribute
    {
        private readonly Features _feature;

        public FeatureToggleAttribute(Features feature)
        {
            _feature = feature;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var configuration = actionContext.Request.GetDependencyScope().GetService(typeof(IFeatureConfiguration)) as IFeatureConfiguration;

            if (configuration != null && !configuration.IsAvailable(_feature))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}