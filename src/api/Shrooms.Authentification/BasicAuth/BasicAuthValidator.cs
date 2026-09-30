using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

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

        public IPrincipal Validate(string userName, string password, CancellationToken cancellationToken, HttpContext httpContext)
        {
            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, UserManager doesn't support CancellationTokens.

            var expectedUserName = _appSettings.BasicUsername;
            var expectedPassword = _appSettings.BasicPassword;

            // Fail closed: unconfigured credentials must never match. Without this, an empty
            // "Authorization: Basic Og==" header authenticated against a blank configuration.
            if (string.IsNullOrEmpty(expectedUserName) || string.IsNullOrEmpty(expectedPassword)
                || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (!FixedTimeEquals(userName, expectedUserName) || !FixedTimeEquals(password, expectedPassword))
            {
                return null;
            }

            var tenantName = httpContext.Items["tenantName"] as string;
            if (string.IsNullOrEmpty(tenantName) || !DoesOrganizationExists(tenantName))
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

        private static bool FixedTimeEquals(string supplied, string expected)
        {
            var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);

            return CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
        }

        private bool DoesOrganizationExists(string tenantName)
        {
            return _dbContext.Set<Organization>().SingleOrDefault(o => o.ShortName == tenantName) != null;
        }
    }
}
