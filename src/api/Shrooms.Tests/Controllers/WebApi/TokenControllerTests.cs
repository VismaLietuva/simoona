using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Controllers;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class TokenControllerTests
    {
        private ShroomsDbContext _dbContext;
        private ShroomsUserManager _userManager;
        private IConfiguration _configuration;
        private TokenController _controller;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ShroomsDbContext(options);

            _dbContext.Set<Organization>().Add(new Organization
            {
                Id = 1,
                ShortName = "Test",
                Name = "TestOrg",
                WelcomeEmail = "Welcome"
            });
            _dbContext.SaveChanges(false);

            var permissionsCache = Substitute.For<ICustomCache<string, IEnumerable<string>>>();
            _userManager = Substitute.ForPartsOf<ShroomsUserManager>(
                Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null,
                permissionsCache);
            _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
                .Returns(Task.FromResult<IList<string>>(new List<string>()));

            _configuration = Substitute.For<IConfiguration>();
            _configuration["JwtSecret"].Returns("test-secret-key-for-unit-tests-min32chars!!");
            _configuration["AccessTokenLifeTimeInHours"].Returns("1");

            _controller = new TokenController(_userManager, _configuration, _dbContext);
        }

        [TearDown]
        public void Cleanup()
        {
            _dbContext?.Dispose();
        }

        [Test]
        public async Task GivenName_WhenBothNamesPopulated_ClaimContainsFullName()
        {
            var user = CreateUser("Jane", "Doe");
            var givenName = await InvokeGenerateJwtAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Jane Doe"));
        }

        [Test]
        public async Task GivenName_WhenFirstNameIsNull_ClaimContainsLastNameOnly()
        {
            var user = CreateUser(null, "Doe");
            var givenName = await InvokeGenerateJwtAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Doe"));
        }

        [Test]
        public async Task GivenName_WhenLastNameIsNull_ClaimContainsFirstNameOnly()
        {
            var user = CreateUser("Jane", null);
            var givenName = await InvokeGenerateJwtAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Jane"));
        }

        [Test]
        public async Task GivenName_WhenBothNamesAreNull_ClaimValueIsEmpty()
        {
            var user = CreateUser(null, null);
            var givenName = await InvokeGenerateJwtAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo(string.Empty));
        }

        private static ApplicationUser CreateUser(string firstName, string lastName)
        {
            return new ApplicationUser
            {
                Id = "test-user",
                UserName = "testuser",
                OrganizationId = 1,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private async Task<string> InvokeGenerateJwtAndExtractGivenName(ApplicationUser user)
        {
            var method = typeof(TokenController).GetMethod(
                "GenerateJwtTokenAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "GenerateJwtTokenAsync method not found via reflection");

            var result = await (Task<object>)method.Invoke(_controller, new object[] { user });
            var token = (string)result.GetType().GetProperty("access_token").GetValue(result);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var givenName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

            return givenName;
        }
    }
}
