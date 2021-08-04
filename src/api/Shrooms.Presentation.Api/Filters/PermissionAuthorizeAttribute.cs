using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.Identity;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Api.Helpers;

namespace Shrooms.Presentation.Api.Filters
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly List<string> _permissions;

        public string Permission { get; set; }

        public PermissionAuthorizeAttribute(string permission = null)
        {
            _permissions = new List<string>();
            if (permission != null)
            {
                _permissions.Add(permission);
            }
        }

        public PermissionAuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions.ToList();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var permissionService = actionContext.Request.GetDependencyScope().GetService(typeof(IPermissionService)) as IPermissionService;

            if (permissionService == null)
            {
                return false;
            }

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = actionContext.Request.GetRequestContext().Principal.Identity.GetUserId(),
                OrganizationId = actionContext.Request.GetRequestContext().Principal.Identity.GetOrganizationId()
            };

            var isPermitted = _permissions.All(p => permissionService.UserHasPermission(userAndOrg, p))
                && (Permission != null && permissionService.UserHasPermission(userAndOrg, Permission) || Permission == null);
            return isPermitted;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // Always throwing 403/Forbidden status is easier to catch with AngularJS. Otherwise on 401/Unauthorized status it would show login dialog.
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Missing permission");
        }
    }
}
