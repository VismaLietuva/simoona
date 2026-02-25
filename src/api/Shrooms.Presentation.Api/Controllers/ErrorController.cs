using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    public class ErrorController : ControllerBase
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions]
        [Route("Error/NotFound")]
        public IActionResult HandleNotFound(string path)
        {
            LogManager.GetCurrentClassLogger().Log(NLog.LogLevel.Info, $"404 Not Found: /{path}");
            return NotFound();
        }
    }
}