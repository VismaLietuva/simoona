using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.Permissions;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.API.Filters
{
    public class HMACAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        private const string OrganizationHeaderName = "Organization";

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var authorizationGuid = GetAuthorizationGuid(context);

            if (authorizationGuid == null)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                return Task.FromResult(0);
            }

            if (context.Request.Headers.Authorization == null)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                return Task.FromResult(0);
            }

            if (context.Request.Headers.Authorization.Parameter != authorizationGuid)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        private string GetAuthorizationGuid(HttpAuthenticationContext context)
        {
            var unitOfWork = context.ActionContext.Request.GetDependencyScope().GetService(typeof(IUnitOfWork2)) as IUnitOfWork2;

            if (context.Request.Headers.Contains(OrganizationHeaderName))
            {
                var organizationName = context.Request.Headers.GetValues(OrganizationHeaderName).FirstOrDefault();

                return unitOfWork.GetDbSet<Organization>()
                    .Where(x => x.ShortName == organizationName)
                    .Select(x => x.BookAppAuthorizationGuid)
                    .FirstOrDefault();
            }

            return null;
        }
    }
}