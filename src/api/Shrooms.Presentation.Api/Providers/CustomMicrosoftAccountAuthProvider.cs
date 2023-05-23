using Autofac;
using Microsoft.Owin.Security.MicrosoftAccount;
using Shrooms.Domain.Services.Organizations;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Shrooms.Presentation.Api.Providers
{
    public class CustomMicrosoftAccountAuthProvider : MicrosoftAccountAuthenticationProvider
    {
        private static readonly Dictionary<string, string> _claimsToAdd = new ()
        {
            { "givenName", ClaimTypes.GivenName },
            { "surname", ClaimTypes.Surname }
        };
        public CustomMicrosoftAccountAuthProvider(ILifetimeScope container)
        {
            OnAuthenticated = async context =>
            {
                foreach (var keyValuePair in context.User)
                {
                    if (_claimsToAdd.TryGetValue(keyValuePair.Key, out var type))
                    {
                        context.Identity.AddClaim(new Claim(type, keyValuePair.Value.ToString()));
                    }
                }

                await Task.CompletedTask;
            };
            OnApplyRedirect = context =>
            {
                using var request = container.BeginLifetimeScope("AutofacWebRequest");
                var org = request.Resolve(typeof(IOrganizationService)) as IOrganizationService;
                var newRedirectUri = context.RedirectUri;
                var organizationName = context.OwinContext.Get<string>("tenantName");

                if (org != null && org.HasOrganizationEmailDomainRestriction(organizationName))
                {
                    var validHostName = org.GetOrganizationHostName(organizationName);
                    var hostDomainParameter = CreateHostDomainParameter(validHostName);
                    newRedirectUri = $"{newRedirectUri}{hostDomainParameter}&organization={organizationName}";
                }

                context.Response.Redirect(newRedirectUri);
            };
        }

        private static string CreateHostDomainParameter(string hostName) => $"&hd={hostName}";
    }
}