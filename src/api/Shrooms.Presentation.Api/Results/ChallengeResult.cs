using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Shrooms.Presentation.Api.Results
{
    public class ChallengeResult : IActionResult
    {
        public ChallengeResult(string loginProvider)
        {
            LoginProvider = loginProvider;
        }

        public string LoginProvider { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await context.HttpContext.ChallengeAsync(LoginProvider);
        }
    }
}