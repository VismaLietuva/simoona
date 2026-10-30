using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.DomainService.WebHookCallbacks
{
    [TestFixture]
    public class UsersAnonymizationWebHookServiceTests
    {
        private UsersAnonymizationWebHookService _usersAnonymizationWebHookService;

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Organization> _organizationsDbSet;
        private DbSet<IdentityUserLogin<string>> _userLoginsDbSet;
        private DbSet<IdentityUserClaim<string>> _userClaimsDbSet;
        private DbSet<IdentityUserToken<string>> _userTokensDbSet;
        private IPictureService _pictureService;
        private MockDbContext _mockDbContext;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _mockDbContext = new MockDbContext();

            _organizationsDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            _organizationsDbSet.SetDbSetDataForAsync(_mockDbContext.Organizations);
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(_mockDbContext.ApplicationUsers);

            _uow = Substitute.For<IUnitOfWork2>();
            _uow.GetDbSet<ApplicationUser>().ReturnsForAnyArgs(_usersDbSet);
            _uow.GetDbSet<Organization>().ReturnsForAnyArgs(_organizationsDbSet);
            _userLoginsDbSet = _uow.MockDbSetForAsync<IdentityUserLogin<string>>();
            _userClaimsDbSet = _uow.MockDbSetForAsync<IdentityUserClaim<string>>();
            _userTokensDbSet = _uow.MockDbSetForAsync<IdentityUserToken<string>>();

            _pictureService = Substitute.For<IPictureService>();

            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            _usersAnonymizationWebHookService = new UsersAnonymizationWebHookService(_uow, _pictureService, configuration);
        }

        [Test]
        public async Task Should_Anonymize_All_Users()
        {
            // Arrange
            var organization = _mockDbContext.Organizations.First();

            var deletedUsers = new List<ApplicationUser>
            {
                new()
                {
                    Id = "d1",
                    OrganizationId = organization.Id,
                    IsDeleted = true,
                    IsAnonymized = false,
                    Modified = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Id = "d2",
                    OrganizationId = organization.Id,
                    IsDeleted = true,
                    IsAnonymized = false,
                    Modified = DateTime.UtcNow.AddDays(-30)
                }
            };

            _usersDbSet.SetDbSetDataForAsync(deletedUsers);

            // Act
            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            // Assert
            Assert.That(_usersDbSet.Any(user => !user.IsAnonymized), Is.False);
        }

        [Test]
        public async Task Should_Delete_The_Photo_Even_When_Other_Records_Reference_It()
        {
            var organization = _mockDbContext.Organizations.First();
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new() { Id = "d1", OrganizationId = organization.Id, IsDeleted = true, Modified = DateTime.UtcNow.AddDays(-30), PictureId = "face.jpg" }
            });

            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            await _pictureService.Received(1).RemoveImageIgnoringReferencesAsync("face.jpg", organization.Id);
            await _pictureService.DidNotReceiveWithAnyArgs().RemoveImageAsync(default, default);
        }

        [Test]
        public void Should_Leave_The_User_For_The_Next_Run_When_The_Photo_Cannot_Be_Deleted()
        {
            var organization = _mockDbContext.Organizations.First();
            var user = new ApplicationUser { Id = "d1", OrganizationId = organization.Id, IsDeleted = true, Modified = DateTime.UtcNow.AddDays(-30), PictureId = "face.jpg" };
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });
            _pictureService.RemoveImageIgnoringReferencesAsync("face.jpg", organization.Id).Returns(Task.FromException(new System.IO.IOException("storage down")));

            Assert.ThrowsAsync<System.IO.IOException>(() => _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName));

            Assert.That(user.IsAnonymized, Is.False);
            Assert.That(user.PictureId, Is.EqualTo("face.jpg"));
            _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        }

        [Test]
        public async Task Should_Overwrite_The_Normalized_Email_And_UserName()
        {
            var organization = _mockDbContext.Organizations.First();
            var user = new ApplicationUser
            {
                Id = "d1",
                OrganizationId = organization.Id,
                IsDeleted = true,
                Modified = DateTime.UtcNow.AddDays(-30),
                Email = "jane.doe@example.com",
                NormalizedEmail = "JANE.DOE@EXAMPLE.COM",
                UserName = "jane.doe@example.com",
                NormalizedUserName = "JANE.DOE@EXAMPLE.COM",
                PasswordHash = "hash",
                AbsentComment = "Visiting Jane's mother in Kaunas"
            };
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });

            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            Assert.That(user.NormalizedEmail, Is.EqualTo(user.Email.ToUpperInvariant()));
            Assert.That(user.NormalizedUserName, Is.EqualTo(user.UserName.ToUpperInvariant()));
            Assert.That(user.NormalizedEmail, Does.Not.Contain("JANE"));
            Assert.That(user.PasswordHash, Is.Null);
            Assert.That(user.AbsentComment, Is.Null);
        }

        [Test]
        public async Task Should_Remove_Only_The_Anonymized_Users_Logins_Claims_And_Tokens()
        {
            var organization = _mockDbContext.Organizations.First();
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new() { Id = "d1", OrganizationId = organization.Id, IsDeleted = true, Modified = DateTime.UtcNow.AddDays(-30) }
            });
            _userLoginsDbSet.SetDbSetDataForAsync(new List<IdentityUserLogin<string>>
            {
                new() { UserId = "d1", LoginProvider = "Google", ProviderKey = "google-sub", ProviderDisplayName = "Google" },
                new() { UserId = "other", LoginProvider = "Google", ProviderKey = "other-sub", ProviderDisplayName = "Google" }
            });
            _userClaimsDbSet.SetDbSetDataForAsync(new List<IdentityUserClaim<string>>
            {
                new() { UserId = "d1", ClaimType = "email", ClaimValue = "jane.doe@example.com" },
                new() { UserId = "other", ClaimType = "email", ClaimValue = "john@example.com" }
            });
            _userTokensDbSet.SetDbSetDataForAsync(new List<IdentityUserToken<string>>
            {
                new() { UserId = "d1", LoginProvider = "Google", Name = "access_token", Value = "secret" },
                new() { UserId = "other", LoginProvider = "Google", Name = "access_token", Value = "other" }
            });

            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            _userLoginsDbSet.Received(1).RemoveRange(Arg.Is<IEnumerable<IdentityUserLogin<string>>>(logins => logins.Single().UserId == "d1"));
            _userClaimsDbSet.Received(1).RemoveRange(Arg.Is<IEnumerable<IdentityUserClaim<string>>>(claims => claims.Single().UserId == "d1"));
            _userTokensDbSet.Received(1).RemoveRange(Arg.Is<IEnumerable<IdentityUserToken<string>>>(tokens => tokens.Single().UserId == "d1"));
        }
    }
}