using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Presentation.Common.Filters
{
    public class HmacAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string OrganizationHeaderName = "Organization";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork2>();

            if (!context.HttpContext.Request.Headers.ContainsKey(OrganizationHeaderName))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var organizationName = context.HttpContext.Request.Headers[OrganizationHeaderName].FirstOrDefault();
            var authorizationGuid = await GetAuthorizationGuidAsync(unitOfWork, organizationName);

            // An organisation without a configured key must not accept an empty header.
            if (string.IsNullOrWhiteSpace(authorizationGuid))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader)
                || string.IsNullOrEmpty(authHeader.ToString())
                || !FixedTimeEquals(authHeader.ToString(), authorizationGuid))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private static bool FixedTimeEquals(string supplied, string expected)
        {
            var suppliedDigest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(supplied));
            var expectedDigest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(expected));
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(suppliedDigest, expectedDigest);
        }

        private static Task<string> GetAuthorizationGuidAsync(IUnitOfWork2 unitOfWork, string organizationName)
        {
            var result = unitOfWork.GetDbSet<Organization>()
                .Where(x => x.ShortName == organizationName)
                .Select(x => x.BookAppAuthorizationGuid)
                .FirstOrDefault();
            return Task.FromResult(result);
        }
    }
}