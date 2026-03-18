// <copyright file="ImpersonateServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Domain.Services.Impersonate;
using Shrooms.Domain.Services.Jwt;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class ImpersonateServiceTests
    {
        private static readonly ApplicationUser TargetUser = new ApplicationUser
        {
            Id = "target-user-id",
            UserName = "targetuser",
            OrganizationId = 1,
        };

        private static readonly ApplicationUser AdminUser = new ApplicationUser
        {
            Id = "admin-user-id",
            UserName = "adminuser",
            OrganizationId = 1,
        };

        private static readonly Organization TestOrganization = new Organization
        {
            Id = 1,
            ShortName = "testorg",
            Name = "Test Organization",
        };

        private ImpersonateService service;
        private ShroomsUserManager userManager;
        private IUnitOfWork2 uow;
        private DbSet<Organization> organizationsDbSet;
        private IConfiguration configuration;

        [SetUp]
        public void SetUp()
        {
            this.uow = Substitute.For<IUnitOfWork2>();
            this.organizationsDbSet = this.uow.MockDbSetForAsync<Organization>(new[] { TestOrganization });

            var configData = new Dictionary<string, string>
            {
                ["JwtSecret"] = "test-secret-key-minimum-32-characters-long!!",
                ["AccessTokenLifeTimeInHours"] = "24",
            };
            this.configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            this.userManager = MockIdentity.MockUserManager(userStore, Substitute.For<IDbContext>());

            this.userManager.FindByNameAsync(TargetUser.UserName).Returns(Task.FromResult(TargetUser));
            this.userManager.FindByNameAsync(AdminUser.UserName).Returns(Task.FromResult(AdminUser));
            this.userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
                .Returns(Task.FromResult<IList<string>>(new List<string> { "User" }));

            var jwtTokenService = new JwtTokenService(this.userManager, this.configuration, this.uow);
            this.service = new ImpersonateService(this.userManager, jwtTokenService);
        }

        // ── ImpersonateUserAsync ──────────────────────────────────────────────
        [Test]
        public async Task ImpersonateUserAsync_SuccessPath_ReturnsTokenWithExpectedClaims()
        {
            var callerPrincipal = BuildAuthenticatedPrincipal(AdminUser.UserName);

            var tokenString = await this.service.ImpersonateUserAsync(TargetUser.UserName, callerPrincipal);

            Assert.That(tokenString, Is.Not.Null.And.Not.Empty);

            var decoded = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                Is.EqualTo(TargetUser.Id));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Is.EqualTo(TargetUser.UserName));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == WebApiConstants.ClaimOrganizationId)?.Value,
                Is.EqualTo("1"));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == WebApiConstants.ClaimOrganizationName)?.Value,
                Is.EqualTo("testorg"));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == DataLayerConstants.ClaimUserImpersonation)?.Value,
                Is.EqualTo(true.ToString()));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == DataLayerConstants.ClaimOriginalUsername)?.Value,
                Is.EqualTo(AdminUser.UserName));
        }

        [Test]
        public void ImpersonateUserAsync_TargetUserNotFound_ThrowsServiceException()
        {
            this.userManager.FindByNameAsync("nonexistent").Returns(Task.FromResult<ApplicationUser>(null));
            var callerPrincipal = BuildAuthenticatedPrincipal(AdminUser.UserName);

            Assert.ThrowsAsync<ServiceException>(() =>
                this.service.ImpersonateUserAsync("nonexistent", callerPrincipal));
        }

        [Test]
        public void ImpersonateUserAsync_UnauthenticatedCaller_ThrowsServiceException()
        {
            // ClaimsIdentity with no authenticationType → IsAuthenticated = false
            var unauthenticated = new ClaimsPrincipal(new ClaimsIdentity());

            Assert.ThrowsAsync<ServiceException>(() =>
                this.service.ImpersonateUserAsync(TargetUser.UserName, unauthenticated));
        }

        // ── RevertImpersonationAsync ──────────────────────────────────────────
        [Test]
        public async Task RevertImpersonationAsync_SuccessPath_ReturnsCleanTokenWithoutImpersonationClaims()
        {
            var impersonatedPrincipal = BuildImpersonatedPrincipal(TargetUser.UserName, AdminUser.UserName);

            var tokenString = await this.service.RevertImpersonationAsync(impersonatedPrincipal);

            Assert.That(tokenString, Is.Not.Null.And.Not.Empty);

            var decoded = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                Is.EqualTo(AdminUser.Id));
            Assert.That(
                decoded.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                Is.EqualTo(AdminUser.UserName));
            Assert.That(
                decoded.Claims.Any(c => c.Type == DataLayerConstants.ClaimUserImpersonation),
                Is.False,
                "Reverted token must not contain UserImpersonation claim.");
            Assert.That(
                decoded.Claims.Any(c => c.Type == DataLayerConstants.ClaimOriginalUsername),
                Is.False,
                "Reverted token must not contain OriginalUsername claim.");
        }

        [Test]
        public void RevertImpersonationAsync_CallerNotImpersonating_ThrowsServiceException()
        {
            var regularPrincipal = BuildAuthenticatedPrincipal(AdminUser.UserName);

            Assert.ThrowsAsync<ServiceException>(() =>
                this.service.RevertImpersonationAsync(regularPrincipal));
        }

        [Test]
        public void RevertImpersonationAsync_OriginalUserNotFound_ThrowsServiceException()
        {
            var impersonatedPrincipal = BuildImpersonatedPrincipal(TargetUser.UserName, "ghost");
            this.userManager.FindByNameAsync("ghost").Returns(Task.FromResult<ApplicationUser>(null));

            Assert.ThrowsAsync<ServiceException>(() =>
                this.service.RevertImpersonationAsync(impersonatedPrincipal));
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private static ClaimsPrincipal BuildAuthenticatedPrincipal(string userName)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, userName) },
                authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal BuildImpersonatedPrincipal(string currentUserName, string originalUserName)
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, currentUserName),
                    new Claim(DataLayerConstants.ClaimUserImpersonation, true.ToString()),
                    new Claim(DataLayerConstants.ClaimOriginalUsername, originalUserName),
                },
                authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
