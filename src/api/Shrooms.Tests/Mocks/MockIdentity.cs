using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Tests.Mocks
{
    public static class MockIdentity
    {
        public static IRoleStore<ApplicationRole> MockRoleStore()
        {
            var mockRoleStore = Substitute.For<IRoleStore<ApplicationRole>>();
            mockRoleStore.FindByNameAsync(Roles.NewUser, default).Returns(Task.FromResult(new ApplicationRole { Name = Roles.NewUser }));
            mockRoleStore.FindByNameAsync(Roles.User, default).Returns(Task.FromResult(new ApplicationRole { Name = Roles.User }));
            mockRoleStore.FindByNameAsync(Roles.Admin, default).Returns(Task.FromResult(new ApplicationRole { Name = Roles.Admin }));
            return mockRoleStore;
        }

        public static IUserStore<ApplicationUser> MockShroomsUserStore(IDbContext context)
        {
            var mockShroomsUserStore = Substitute.For<IUserStore<ApplicationUser>>();
            mockShroomsUserStore.FindByNameAsync(string.Empty, default).Returns(Task.FromResult((ApplicationUser)null));
            mockShroomsUserStore.FindByNameAsync("user", default).Returns(Task.FromResult(new ApplicationUser { UserName = "user", Email = "user@test.lt" }));
            mockShroomsUserStore.FindByNameAsync("admin", default).Returns(Task.FromResult(new ApplicationUser { UserName = "admin", Email = "admin@test.lt" }));
            mockShroomsUserStore.FindByIdAsync(string.Empty, default).Returns(Task.FromResult(new ApplicationUser { UserName = "test", Email = "test@test.lt" }));
            return mockShroomsUserStore;
        }

        public static ShroomsUserManager MockUserManager(IUserStore<ApplicationUser> userStore, IDbContext ctx)
        {
            var customCacheMock = Substitute.For<ICustomCache<string, IEnumerable<string>>>();
            var mockUserManager = Substitute.For<ShroomsUserManager>(
                userStore,
                Substitute.For<IOptions<IdentityOptions>>(),
                null,
                null,
                null,
                null,
                null,
                null,
                Substitute.For<ILogger<UserManager<ApplicationUser>>>(),
                customCacheMock);
            mockUserManager.FindByNameAsync(string.Empty).Returns(Task.FromResult((ApplicationUser)null));
            mockUserManager.FindByNameAsync("user").Returns(Task.FromResult(new ApplicationUser { UserName = "user", Email = "user@test.lt" }));
            mockUserManager.FindByNameAsync("admin").Returns(Task.FromResult(new ApplicationUser { UserName = "admin", Email = "admin@test.lt" }));
            mockUserManager.FindByIdAsync(string.Empty).Returns(Task.FromResult(new ApplicationUser { UserName = "test", Email = "test@test.lt" }));
            return mockUserManager;
        }

        public static ShroomsRoleManager MockRoleManager(IRoleStore<ApplicationRole> roleStore)
        {
            var mockRoleManager = Substitute.For<ShroomsRoleManager>(roleStore, null, null, null, null);
            mockRoleManager.FindByNameAsync(Roles.NewUser).Returns(Task.FromResult(new ApplicationRole { Name = Roles.NewUser }));
            mockRoleManager.FindByNameAsync(Roles.User).Returns(Task.FromResult(new ApplicationRole { Name = Roles.User }));
            mockRoleManager.FindByNameAsync(Roles.Admin).Returns(Task.FromResult(new ApplicationRole { Name = Roles.Admin }));
            return mockRoleManager;
        }

        public static void MockIdentityAndPrincipal(ControllerBase controller)
        {
            var mockPrincipal = GetPrincipalMock();
            var httpContext = new DefaultHttpContext
            {
                User = (ClaimsPrincipal)mockPrincipal
            };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public static IPrincipal GetPrincipalMock()
        {
            var claim = new Claim("Id", "1");
            var orgClaim = new Claim(WebApiConstants.ClaimOrganizationId, "1");

            var mockIdentity = Substitute.For<ClaimsIdentity>();
            mockIdentity.FindFirst(Arg.Any<string>()).Returns(claim);
            mockIdentity.FindFirst(WebApiConstants.ClaimOrganizationId).Returns(orgClaim);

            var mockPrincipal = Substitute.For<IPrincipal>();
            mockPrincipal.Identity.Returns(mockIdentity);

            return mockPrincipal;
        }

        public static IPrincipal GetPrincipalMock(string id, string name, string[] roles)
        {
            var claims = new List<Claim>
            {
                new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", name),
                new("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", id),
                new(WebApiConstants.ClaimOrganizationId, "1")
            };

            var genericIdentity = new GenericIdentity(string.Empty);
            genericIdentity.AddClaims(claims);

            var genericPrincipal = new GenericPrincipal(genericIdentity, roles);
            return genericPrincipal;
        }
    }
}
