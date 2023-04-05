using Autofac;
using Microsoft.Owin.Security.MicrosoftAccount;
using Shrooms.Domain.Services.Organizations;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Claim = System.Security.Claims.Claim;
namespace Shrooms.Presentation.Api.Providers
{
    public class CustomMicrosoftAccountAuthProvider : MicrosoftAccountAuthenticationProvider
    {
        public CustomMicrosoftAccountAuthProvider(ILifetimeScope container)
        {
            OnAuthenticated = async context =>
            {
                foreach (var claim in context.User)
                {
                    if (claim.Key.Equals("first_name"))
                    {
                        var claimValue = claim.Value.ToString();
                        context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, claimValue));
                    }
                    else if (claim.Key.Equals("last_name"))
                    {
                        var claimValue = claim.Value.ToString();
                        context.Identity.AddClaim(new Claim(ClaimTypes.Surname, claimValue));
                    }
                    else if (claim.Key.Equals("picture"))
                    {
                        var json = JObject.Parse(claim.Value.ToString());
                        bool.TryParse(json.SelectToken("data.is_silhouette").ToString(), out var isDefaultImage);
                        if (isDefaultImage == false)
                        {
                            context.Identity.AddClaim(new Claim("picture", json.SelectToken("data.url").ToString()));
                        }
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