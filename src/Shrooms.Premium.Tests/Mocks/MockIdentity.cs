using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using NSubstitute;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Premium.Tests.Mocks
{
    public static class MockIdentity
    {
        public static IRoleStore<ApplicationRole, string> MockRoleStore()
        {
            var mockRoleStore = Substitute.For<IRoleStore<ApplicationRole>>();
            mockRoleStore.FindByNameAsync(Roles.NewUser).Returns(Task.FromResult(new ApplicationRole { Name = Roles.NewUser }));
            mockRoleStore.FindByNameAsync(Roles.User).Returns(Task.FromResult(new ApplicationRole { Name = Roles.User }));
            mockRoleStore.FindByNameAsync(Roles.Admin).Returns(Task.FromResult(new ApplicationRole { Name = Roles.Admin }));
            return mockRoleStore;
        }

        public static ShroomsUserStore MockShroomsUserStore(IDbContext context)
        {
            var mockShroomsUserStore = Substitute.For<ShroomsUserStore>(context);
            var mockPermissionService = Substitute.For<IPermissionService>();
            mockShroomsUserStore.FindByNameAsync(string.Empty).Returns(Task.FromResult((ApplicationUser)null));
            mockShroomsUserStore.FindByNameAsync("user").Returns(Task.FromResult(new ApplicationUser { UserName = "user", Email = "user@test.lt" }));
            mockShroomsUserStore.FindByNameAsync("admin").Returns(Task.FromResult(new ApplicationUser { UserName = "admin", Email = "admin@test.lt" }));
            mockShroomsUserStore.FindByIdAsync(string.Empty).Returns(Task.FromResult(new ApplicationUser { UserName = "test", Email = "test@test.lt" }));
            mockPermissionService.UserHasPermission(new UserAndOrganizationDTO { UserId = string.Empty }, AdministrationPermissions.ApplicationUser).Returns(true);
            return mockShroomsUserStore;
        }

        public static ShroomsUserManager MockUserManager(IUserStore<ApplicationUser> userStore, IDbContext ctx)
        {
            var claimsIdentityFactory = Substitute.For<ShroomsClaimsIdentityFactory>(ctx);
            var dataProtectionProvider = Substitute.For<IDataProtectionProvider>();
            var customCacheMock = Substitute.For<ICustomCache<string, IEnumerable<string>>>();
            var mockUserManager = Substitute.For<ShroomsUserManager>(userStore, dataProtectionProvider, null, claimsIdentityFactory, customCacheMock);
            mockUserManager.FindByNameAsync(string.Empty).Returns(Task.FromResult((ApplicationUser)null));
            mockUserManager.FindByNameAsync("user").Returns(Task.FromResult(new ApplicationUser { UserName = "user", Email = "user@test.lt" }));
            mockUserManager.FindByNameAsync("admin").Returns(Task.FromResult(new ApplicationUser { UserName = "admin", Email = "admin@test.lt" }));
            mockUserManager.FindByIdAsync(string.Empty).Returns(Task.FromResult(new ApplicationUser { UserName = "test", Email = "test@test.lt" }));
            return mockUserManager;
        }

        public static ShroomsRoleManager MockRoleManager(IRoleStore<ApplicationRole, string> roleStore)
        {
            var mockRoleManager = Substitute.For<ShroomsRoleManager>(roleStore);
            mockRoleManager.FindByNameAsync(Roles.NewUser).Returns(Task.FromResult(new ApplicationRole { Name = Roles.NewUser }));
            mockRoleManager.FindByNameAsync(Roles.User).Returns(Task.FromResult(new ApplicationRole { Name = Roles.User }));
            mockRoleManager.FindByNameAsync(Roles.Admin).Returns(Task.FromResult(new ApplicationRole { Name = Roles.Admin }));
            return mockRoleManager;
        }

        public static void MockIdentityAndPrincipal(ApiController controller)
        {
            var mockPrincipal = GetPrincipalMock();
            controller.RequestContext.Principal = mockPrincipal;
            controller.User = mockPrincipal;
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
            List<Claim> claims = new List<Claim>
            {
                            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", name),
                            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", id),
                            new Claim(WebApiConstants.ClaimOrganizationId, "1")
            };
            var genericIdentity = new GenericIdentity(string.Empty);
            genericIdentity.AddClaims(claims);
            var genericPrincipal = new GenericPrincipal(genericIdentity, roles);
            return genericPrincipal;
        }
    }
}