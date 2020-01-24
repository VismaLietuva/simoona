using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Microsoft.Owin;
using Shrooms.Constants.WebApi;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.Authentification.BasicAuth
{
    public class BasicAuthValidator : IBasicAuthValidator
    {
        private readonly IApplicationSettings _appSettings;
        private readonly IDbContext _dbContext;

        public BasicAuthValidator(IApplicationSettings appSettings, IDbContext dbContext)
        {
            _appSettings = appSettings;
            _dbContext = dbContext;
        }

        public IPrincipal Validate(string userName, string password, CancellationToken cancellationToken, IOwinContext owinContext)
        {
            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, UserManager doesn't support CancellationTokens.

            var tenantName = owinContext.Get<string>("tenantName");

            if (userName != _appSettings.BasicUsername && password != _appSettings.BasicPassword && DoesOrganizationExists(tenantName))
            {
                return null;
            }

            // Create a ClaimsIdentity with all the claims for this user.
            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, IClaimsIdenityFactory doesn't support CancellationTokens.

            var claims = new List<Claim>
            {
                new Claim("name", "app"),
                new Claim("role", "scheduler-webhook"),
                new Claim(WebApiConstants.ClaimOrganizationName, tenantName)
            };

            var identity = new ClaimsIdentity(claims, "Basic", "name", "role");
            return new ClaimsPrincipal(identity);
        }

        private bool DoesOrganizationExists(string tenantName)
        {
            return _dbContext.Set<Organization>().SingleOrDefault(o => o.ShortName == tenantName) != null;
        }
    }
}