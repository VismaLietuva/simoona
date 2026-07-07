using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shrooms.Presentation.Common.Filters
{
    public abstract class BasicAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string Realm { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var parameter = authHeader.Substring("Basic ".Length).Trim();
            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userNameAndPassword = ExtractUserNameAndPassword(parameter);
            if (userNameAndPassword == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var principal = await AuthenticateAsync(userNameAndPassword.Item1, userNameAndPassword.Item2,
                CancellationToken.None, context.HttpContext);

            if (principal == null)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                context.HttpContext.User = principal as System.Security.Claims.ClaimsPrincipal
                    ?? new System.Security.Claims.ClaimsPrincipal(principal.Identity);
            }
        }

        protected abstract Task<IPrincipal> AuthenticateAsync(string userName, string password,
            CancellationToken cancellationToken, HttpContext httpContext);

        private static Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;
            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            var encoding = (Encoding)Encoding.ASCII.Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            string decodedCredentials;
            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (string.IsNullOrEmpty(decodedCredentials))
            {
                return null;
            }

            var colonIndex = decodedCredentials.IndexOf(':');
            if (colonIndex == -1)
            {
                return null;
            }

            var userName = decodedCredentials.Substring(0, colonIndex);
            var password = decodedCredentials.Substring(colonIndex + 1);
            return new Tuple<string, string>(userName, password);
        }
    }
}
