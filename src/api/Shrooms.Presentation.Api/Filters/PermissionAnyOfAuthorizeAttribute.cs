using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Presentation.Api.Filters
{
    public class PermissionAnyOfAuthorizeAttribute : Attribute, IAuthorizationFilter
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

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User?.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = context.HttpContext.User.Identity.GetUserId(),
                OrganizationId = context.HttpContext.User.Identity.GetOrganizationId()
            };

            var isPermitted = _permissions.Any(p => permissionService != null && permissionService.UserHasPermission(userAndOrg, p));
            if (!isPermitted)
            {
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}