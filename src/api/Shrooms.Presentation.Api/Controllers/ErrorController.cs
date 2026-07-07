using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Shrooms.Presentation.Api.Controllers
{
    [AllowAnonymous]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions]
        [Route("Error/NotFound")]
        public IActionResult HandleNotFound(string path)
        {
            _logger.LogInformation("404 Not Found: /{Path}", path);
            return NotFound();
        }
    }
}
