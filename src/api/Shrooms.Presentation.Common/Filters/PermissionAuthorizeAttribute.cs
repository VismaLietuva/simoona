using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Helpers;

namespace Shrooms.Presentation.Common.Filters
{
    public class PermissionAuthorizeAttribute : Attribute, IAuthorizationFilter
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

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();

            if (permissionService == null)
            {
                context.Result = new ObjectResult("Missing permission") { StatusCode = 403 };
                return;
            }

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = context.HttpContext.User.Identity.GetUserId(),
                OrganizationId = context.HttpContext.User.Identity.GetOrganizationId()
            };

            var isPermitted = _permissions.All(p => permissionService.UserHasPermission(userAndOrg, p))
                && (Permission == null || permissionService.UserHasPermission(userAndOrg, Permission));

            if (!isPermitted)
            {
                context.Result = new ObjectResult("Missing permission") { StatusCode = 403 };
            }
        }
    }
}
