using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Shrooms.Presentation.Api.Filters
{
    public class PermissionAnyOfAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly List<string> _permissions = new List<string>();

        public PermissionAnyOfAuthorizeAttribute(string permission = null)
        {
            if (permission != null)
            {
                _permissions.Add(permission);
            }
        }

        public PermissionAnyOfAuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions.ToList();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var permissionService = actionContext.Request.GetDependencyScope().GetService(typeof(IPermissionService)) as IPermissionService;

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = actionContext.Request.GetRequestContext().Principal.Identity.GetUserId(),
                OrganizationId = actionContext.Request.GetRequestContext().Principal.Identity.GetOrganizationId()
            };

            var isPermitted = _permissions.Any(p => permissionService != null && permissionService.UserHasPermission(userAndOrg, p));
            return isPermitted;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // Always throwing 403/Forbidden status is easier to catch with AngularJS. Otherwise on 401/Unauthorized status it would show login dialog.
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Missing permission");
        }
    }
}
