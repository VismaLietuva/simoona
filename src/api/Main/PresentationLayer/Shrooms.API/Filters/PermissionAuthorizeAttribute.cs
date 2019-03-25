using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.Identity;
using Shrooms.API.Helpers;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.API.Filters
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public string Permission { get; set; }
        public string Scope { get; set; }
        public IList<string> AddToDefaultRoles { get; set; }

        public string AddToDefaultRole
        {
            get { return AddToDefaultRoles.FirstOrDefault(); }
            set { AddToDefaultRoles.Add(value); }
        }

        public PermissionAuthorizeAttribute(string permission = null, string scope = null)
        {
            Permission = permission;
            Scope = scope;
            AddToDefaultRoles = AddToDefaultRoles ?? new List<string>();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var permissionService = actionContext.Request.GetDependencyScope().GetService(typeof(IPermissionService)) as IPermissionService;

            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = actionContext.Request.GetRequestContext().Principal.Identity.GetUserId(),
                OrganizationId = actionContext.Request.GetRequestContext().Principal.Identity.GetOrganizationId()
            };

            var isPermitted = permissionService.UserHasPermission(userAndOrg, Permission);
            return isPermitted;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // Always throwing 403/Forbidden status is easier to catch with AngularJS. Otherwise on 401/Unauthorized status it would show login dialog.
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Missing permission");
        }
    }
}
