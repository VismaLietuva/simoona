using Autofac;
using Microsoft.Owin.Security.MicrosoftAccount;
using Shrooms.Domain.Services.Organizations;
namespace Shrooms.Presentation.Api.Providers
{
    public class CustomMicrosoftAccountAuthProvider : MicrosoftAccountAuthenticationProvider
    {
        public CustomMicrosoftAccountAuthProvider(ILifetimeScope container)
        {
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