using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Jwt;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class TokenControllerTests
    {
        private ShroomsDbContext _dbContext;
        private ShroomsUserManager _userManager;
        private IConfiguration _configuration;
        private JwtTokenService _jwtTokenService;

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

            var uow = Substitute.For<IUnitOfWork2>();
            uow.GetDbSet<Organization>().Returns(_dbContext.Set<Organization>());

            _jwtTokenService = new JwtTokenService(_userManager, _configuration, uow);
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
            var givenName = await GenerateTokenAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Jane Doe"));
        }

        [Test]
        public async Task GivenName_WhenFirstNameIsNull_ClaimContainsLastNameOnly()
        {
            var user = CreateUser(null, "Doe");
            var givenName = await GenerateTokenAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Doe"));
        }

        [Test]
        public async Task GivenName_WhenLastNameIsNull_ClaimContainsFirstNameOnly()
        {
            var user = CreateUser("Jane", null);
            var givenName = await GenerateTokenAndExtractGivenName(user);
            Assert.That(givenName, Is.EqualTo("Jane"));
        }

        [Test]
        public async Task GivenName_WhenBothNamesAreNull_ClaimValueIsEmpty()
        {
            var user = CreateUser(null, null);
            var givenName = await GenerateTokenAndExtractGivenName(user);
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

        private async Task<string> GenerateTokenAndExtractGivenName(ApplicationUser user)
        {
            var result = await _jwtTokenService.GenerateTokenAsync(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);
            return jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
        }
    }
}
