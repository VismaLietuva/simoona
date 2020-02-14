using System.Threading.Tasks;
using Autofac;
using Microsoft.Owin.Security.Google;
using Newtonsoft.Json.Linq;
using Shrooms.Domain.Services.Organizations;

namespace Shrooms.Presentation.Api.Providers
{
    public class CustomGoogleAuthProvider : GoogleOAuth2AuthenticationProvider
    {
        public CustomGoogleAuthProvider(IContainer ioc)
        {
            OnAuthenticated = async context =>
            {
                foreach (var claim in context.User)
                {
                    if (!claim.Key.Equals("image"))
                    {
                        continue;
                    }

                    var json = JObject.Parse(claim.Value.ToString());
                    bool.TryParse(json.SelectToken("isDefault").ToString(), out var isDefaultImage);
                    if (isDefaultImage != false)
                    {
                        continue;
                    }

                    var plainUri = json.SelectToken("url").ToString().Split('?')[0];
                    context.Identity.AddClaim(new System.Security.Claims.Claim("picture", plainUri));
                }

                await Task.CompletedTask;
            };
            OnApplyRedirect = context =>
            {
                using (var webReq = ioc.BeginLifetimeScope("AutofacWebRequest"))
                {
                    var org = webReq.Resolve(typeof(IOrganizationService)) as IOrganizationService;
                    var newRedirectUri = context.RedirectUri;
                    var organizationName = context.OwinContext.Get<string>("tenantName");
                    if (org.HasOrganizationEmailDomainRestriction(organizationName))
                    {
                        var validHostName = org.GetOrganizationHostName(organizationName);
                        var hostDomainParameter = CreateHostDomainParameter(validHostName);
                        newRedirectUri = $"{newRedirectUri}{hostDomainParameter}&organization={organizationName}";
                    }

                    context.Response.Redirect(newRedirectUri);
                }
            };
        }

        private static string CreateHostDomainParameter(string hostName) => $"&hd={hostName}";
    }
}