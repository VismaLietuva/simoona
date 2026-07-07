using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Shrooms.Authentification.BasicAuth
{
    public interface IBasicAuthValidator
    {
        IPrincipal Validate(string userName, string password, CancellationToken cancellationToken, HttpContext httpContext);
    }
}