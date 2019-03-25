using System.Security.Principal;
using System.Threading;
using Microsoft.Owin;

namespace Shrooms.Authorization.BasicAuth
{
    public interface IBasicAuthValidator
    {
        IPrincipal Validate(string userName, string password, CancellationToken cancellationToken, IOwinContext owinContext);
    }
}