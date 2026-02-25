using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IFeatureConfiguration>();

            if (configuration != null && !configuration.IsAvailable(_feature))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}