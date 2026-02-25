using Microsoft.AspNetCore.Mvc.Filters;

namespace Shrooms.Presentation.Api.GeneralCode.SerializationIgnorer
{
    public class SerializationIgnoreAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // TODO: Hook into response serialization if needed
        }
    }
}