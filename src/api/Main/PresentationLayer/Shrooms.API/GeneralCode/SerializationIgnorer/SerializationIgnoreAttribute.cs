using System.Web.Http.Filters;

namespace Shrooms.API.GeneralCode.SerializationIgnorer
{
    public class SerializationIgnoreAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var response = actionExecutedContext.Response; //.Content as ObjectContent;
            if (response != null)
            {
                SerializationIgnorer.ModifyViewModel(response.Content);
            }
        }
    }
}