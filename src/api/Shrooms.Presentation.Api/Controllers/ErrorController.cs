using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using NLog;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    public class ErrorController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions]
        public IHttpActionResult NotFound(string path)
        {
            if (Request == null)
            {
                Request = new HttpRequestMessage();
            }

            LogManager.GetCurrentClassLogger().Log(LogLevel.Info, new HttpException((int)HttpStatusCode.NotFound, $"404 Not Found: /{path}"));
            return NotFound();
        }
    }
}