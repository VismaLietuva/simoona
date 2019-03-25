using System.Web;
using System.Web.Http;
using NLog;

namespace Shrooms.API.Controllers
{
    [AllowAnonymous]
    public class ErrorController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions]
        public IHttpActionResult NotFound(string path)
        {
            if (Request == null)
            {
                Request = new System.Net.Http.HttpRequestMessage();
            }

            LogManager.GetCurrentClassLogger().Log(LogLevel.Info, new HttpException(404, "404 Not Found: /" + path));
            return NotFound();
        }
    }
}