using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Authentification.BasicAuth;

namespace Shrooms.Presentation.Common.Filters
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            var basicAuthValidator = Request.GetDependencyScope().GetService(typeof(IBasicAuthValidator)) as IBasicAuthValidator;

            return await Task.FromResult(basicAuthValidator?.Validate(userName, password, cancellationToken, Request.GetOwinContext()));
        }
    }
}
