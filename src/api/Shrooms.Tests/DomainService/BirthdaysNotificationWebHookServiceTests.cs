using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class BirthdaysNotificationWebHookServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Organization> _organizationsDbSet;
        private IMailingService _mailingService;
        private IRoleService _roleService;
        private IMailTemplate _mailTemplate;
        private IApplicationSettings _appSettings;
        private IBirthdaysNotificationWebHookService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _organizationsDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();

            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            _uow.GetDbSet<Organization>().Returns(_organizationsDbSet);

            _mailingService = Substitute.For<IMailingService>();
            _roleService = Substitute.For<IRoleService>();
            _mailTemplate = Substitute.For<IMailTemplate>();
            _appSettings = Substitute.For<IApplicationSettings>();

            _sut = new BirthdaysNotificationWebHookService(_uow, _mailingService, _roleService, _mailTemplate, _appSettings);
        }

        [Test]
        public async Task SendNotificationsAsync_WithoutBirthdayUsers_DoesNotSendEmail()
        {
            // Arrange
            var organizationName = "TestOrg";
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>());

            // Act
            await _sut.SendNotificationsAsync(organizationName);

            // Assert
            await _mailingService.DidNotReceive()
                .SendEmailAsync(Arg.Any<EmailDto>());
        }

        [Test]
        public async Task SendNotificationsAsync_WithBirthdayUsers_SendsEmail()
        {
            // Arrange
            var organizationName = "TestOrg";
            var today = DateTime.UtcNow;

            var birthdayUser = new ApplicationUser
            {
                Id = "user1",
                FirstName = "John",
                LastName = "Doe",
                BirthDay = new DateTime(1990, today.Month, today.Day),
                PictureId = "pic1"
            };

            var organization = new Organization
            {
                Id = 1,
                ShortName = organizationName
            };

            var adminEmails = new List<string> { "admin@test.com" };
            var expectedEmailContent = "<html>Birthday Notification</html>";

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { birthdayUser });
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization> { organization }.AsQueryable());

            _roleService.GetAdministrationRoleEmailsAsync(1)
                .Returns(Task.FromResult(adminEmails.AsEnumerable()));

            _appSettings.UserNotificationSettingsUrl(organizationName)
                .Returns("http://settings.test.com");

            _appSettings.PictureUrl(organizationName, "pic1")
                .Returns("http://pictures.test.com/pic1.jpg");

            _appSettings.UserProfileUrl(organizationName, "user1")
                .Returns("http://profile.test.com/user1");

            _mailTemplate.GenerateAsync(
                Arg.Any<BirthdaysNotificationTemplateViewModel>(),
                Arg.Any<string>())
                .Returns(Task.FromResult(expectedEmailContent));

            // Act
            await _sut.SendNotificationsAsync(organizationName);

            // Assert
            await _mailTemplate.Received(1)
                .GenerateAsync(
                    Arg.Any<BirthdaysNotificationTemplateViewModel>(),
                    Arg.Is<string>(key => key == EmailTemplateCacheKeys.BirthdaysNotification));

            await _mailingService.Received(1)
                .SendEmailAsync(Arg.Is<EmailDto>(email =>
                    email.Recipients.SequenceEqual(adminEmails) &&
                    email.Body == expectedEmailContent));
        }

        [Test]
        public async Task SendNotificationsAsync_WithMultipleBirthdayUsers_SendsEmailWithAllUsers()
        {
            // Arrange
            var organizationName = "TestOrg";
            var today = DateTime.UtcNow;

            var birthdayUser1 = new ApplicationUser
            {
                Id = "user1",
                FirstName = "John",
                LastName = "Doe",
                BirthDay = new DateTime(1990, today.Month, today.Day),
                PictureId = "pic1"
            };

            var birthdayUser2 = new ApplicationUser
            {
                Id = "user2",
                FirstName = "Jane",
                LastName = "Smith",
                BirthDay = new DateTime(1995, today.Month, today.Day),
                PictureId = "pic2"
            };

            var organization = new Organization
            {
                Id = 1,
                ShortName = organizationName
            };

            var adminEmails = new List<string> { "admin@test.com" };
            var expectedEmailContent = "<html>Birthday Notification</html>";

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { birthdayUser1, birthdayUser2 });
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization> { organization }.AsQueryable());

            _roleService.GetAdministrationRoleEmailsAsync(1)
                .Returns(Task.FromResult(adminEmails.AsEnumerable()));

            _appSettings.UserNotificationSettingsUrl(organizationName)
                .Returns("http://settings.test.com");

            _appSettings.PictureUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => $"http://pictures.test.com/{x.ArgAt<string>(1)}.jpg");

            _appSettings.UserProfileUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => $"http://profile.test.com/{x.ArgAt<string>(1)}");

            _mailTemplate.GenerateAsync(
                Arg.Any<BirthdaysNotificationTemplateViewModel>(),
                Arg.Any<string>())
                .Returns(Task.FromResult(expectedEmailContent));

            // Act
            await _sut.SendNotificationsAsync(organizationName);

            // Assert
            await _mailTemplate.Received(1)
                .GenerateAsync(
                    Arg.Any<BirthdaysNotificationTemplateViewModel>(),
                    Arg.Is<string>(key => key == EmailTemplateCacheKeys.BirthdaysNotification));

            await _mailingService.Received(1)
                .SendEmailAsync(Arg.Any<EmailDto>());
        }
    }
}
