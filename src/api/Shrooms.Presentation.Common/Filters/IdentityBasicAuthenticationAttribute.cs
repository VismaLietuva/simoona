using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Authentification.BasicAuth;

namespace Shrooms.Presentation.Common.Filters
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override Task<IPrincipal> AuthenticateAsync(string userName, string password,
            CancellationToken cancellationToken, HttpContext httpContext)
        {
            var basicAuthValidator = httpContext.RequestServices.GetService<IBasicAuthValidator>();
            return Task.FromResult(basicAuthValidator?.Validate(userName, password, cancellationToken, httpContext));
        }
    }
}
