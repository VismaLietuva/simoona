using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Filters;

namespace Shrooms.Tests.Filters
{
    [TestFixture]
    public class PermissionAuthorizeAttributeTests
    {
        private const string UserId = "user-1";
        private const int OrganizationId = 42;

        private IPermissionService _permissionService;

        [SetUp]
        public void SetUp()
        {
            _permissionService = Substitute.For<IPermissionService>();
        }

        [Test]
        public void OnAuthorization_UnauthenticatedUser_ReturnsUnauthorized()
        {
            var attribute = new PermissionAuthorizeAttribute { Permission = "KUDOS_BASIC" };
            var context = BuildContext(isAuthenticated: false);

            attribute.OnAuthorization(context);

            Assert.That(context.Result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public void OnAuthorization_MissingPermissionService_Returns403()
        {
            var attribute = new PermissionAuthorizeAttribute { Permission = "KUDOS_BASIC" };
            var context = BuildContext(permissionService: null);

            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        // Single permission: [PermissionAuthorize(Permission = "X")]
        [Test]
        public void OnAuthorization_SinglePermission_UserHasIt_Passes()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_BASIC").Returns(true);
            var attribute = new PermissionAuthorizeAttribute { Permission = "KUDOS_BASIC" };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        [Test]
        public void OnAuthorization_SinglePermission_UserLacksIt_Returns403()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_BASIC").Returns(false);
            var attribute = new PermissionAuthorizeAttribute { Permission = "KUDOS_BASIC" };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        // AND semantics via params ctor: [PermissionAuthorize("A", "B")]
        [Test]
        public void OnAuthorization_ParamsAllHeld_Passes()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "A").Returns(true);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "B").Returns(true);
            var attribute = new PermissionAuthorizeAttribute("A", "B");

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        [Test]
        public void OnAuthorization_ParamsOneMissing_Returns403()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "A").Returns(true);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "B").Returns(false);
            var attribute = new PermissionAuthorizeAttribute("A", "B");

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        // AnyOf: user needs at least one of the listed permissions.
        [Test]
        public void OnAuthorization_AnyOf_UserHasFirst_Passes()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_BASIC").Returns(true);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_ADMINISTRATION").Returns(false);
            var attribute = new PermissionAuthorizeAttribute
            {
                AnyOf = new[] { "KUDOS_BASIC", "KUDOS_ADMINISTRATION" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        [Test]
        public void OnAuthorization_AnyOf_UserHasSecond_Passes()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_BASIC").Returns(false);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_ADMINISTRATION").Returns(true);
            var attribute = new PermissionAuthorizeAttribute
            {
                AnyOf = new[] { "KUDOS_BASIC", "KUDOS_ADMINISTRATION" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        [Test]
        public void OnAuthorization_AnyOf_UserHasNone_Returns403()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>()).Returns(false);
            var attribute = new PermissionAuthorizeAttribute
            {
                AnyOf = new[] { "KUDOS_BASIC", "KUDOS_ADMINISTRATION" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        // Mixed: Permission + AnyOf. Both must be satisfied.
        [Test]
        public void OnAuthorization_PermissionAndAnyOf_BothSatisfied_Passes()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "REQUIRED").Returns(true);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "A").Returns(false);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "B").Returns(true);
            var attribute = new PermissionAuthorizeAttribute
            {
                Permission = "REQUIRED",
                AnyOf = new[] { "A", "B" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        [Test]
        public void OnAuthorization_PermissionAndAnyOf_PermissionMissing_Returns403()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "REQUIRED").Returns(false);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "A").Returns(true);
            var attribute = new PermissionAuthorizeAttribute
            {
                Permission = "REQUIRED",
                AnyOf = new[] { "A", "B" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        [Test]
        public void OnAuthorization_PermissionAndAnyOf_AnyOfMissing_Returns403()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "REQUIRED").Returns(true);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), Arg.Is<string>(s => s == "A" || s == "B")).Returns(false);
            var attribute = new PermissionAuthorizeAttribute
            {
                Permission = "REQUIRED",
                AnyOf = new[] { "A", "B" },
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertForbidden(context);
        }

        // Empty AnyOf should be treated as "not specified" — otherwise it would
        // always fail, which is a footgun.
        [Test]
        public void OnAuthorization_EmptyAnyOf_IsIgnored()
        {
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDto>(), "KUDOS_BASIC").Returns(true);
            var attribute = new PermissionAuthorizeAttribute
            {
                Permission = "KUDOS_BASIC",
                AnyOf = Array.Empty<string>(),
            };

            var context = BuildContext();
            attribute.OnAuthorization(context);

            AssertPermitted(context);
        }

        private static void AssertPermitted(AuthorizationFilterContext context)
        {
            Assert.That(context.Result, Is.Null, "Expected the filter to leave Result unset (permitted).");
        }

        private static void AssertForbidden(AuthorizationFilterContext context)
        {
            Assert.That(context.Result, Is.InstanceOf<ObjectResult>());
            var result = (ObjectResult)context.Result;
            Assert.That(result.StatusCode, Is.EqualTo(403));
        }

        private AuthorizationFilterContext BuildContext(
            bool isAuthenticated = true,
            IPermissionService permissionService = null)
        {
            permissionService ??= _permissionService;

            var httpContext = new DefaultHttpContext();

            if (isAuthenticated)
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, UserId),
                        new Claim(WebApiConstants.ClaimOrganizationId, OrganizationId.ToString()),
                    },
                    authenticationType: "Test");
                httpContext.User = new ClaimsPrincipal(identity);
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            httpContext.RequestServices = new StubServiceProvider(permissionService);

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        private sealed class StubServiceProvider : IServiceProvider
        {
            private readonly IPermissionService _permissionService;

            public StubServiceProvider(IPermissionService permissionService)
            {
                _permissionService = permissionService;
            }

            public object GetService(Type serviceType)
            {
                return serviceType == typeof(IPermissionService) ? _permissionService : null;
            }
        }
    }
}
