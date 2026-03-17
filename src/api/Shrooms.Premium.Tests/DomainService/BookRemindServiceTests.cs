using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.Domain.Services.Books;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class BookRemindServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<BookLog> _bookLogsDbSet;
        private IApplicationSettings _appSettings;
        private IUserService _userService;
        private IMailTemplate _mailTemplate;
        private IMailingService _mailingService;
        private IOrganizationService _organizationService;
        private ILogger _logger;
        private IBookRemindService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _bookLogsDbSet = Substitute.For<DbSet<BookLog>, IQueryable<BookLog>, IAsyncEnumerable<BookLog>>();

            _uow.GetDbSet<BookLog>().Returns(_bookLogsDbSet);

            _appSettings = Substitute.For<IApplicationSettings>();
            _userService = Substitute.For<IUserService>();
            _mailTemplate = Substitute.For<IMailTemplate>();
            _mailingService = Substitute.For<IMailingService>();
            _organizationService = Substitute.For<IOrganizationService>();
            _logger = Substitute.For<ILogger>();

            _sut = new BookRemindService(
                _uow,
                _organizationService,
                _appSettings,
                _userService,
                _mailTemplate,
                _mailingService,
                _logger);
        }

        [Test]
        public async Task RemindAboutBooksAsync_WithoutOverdueBooks_DoesNotSendEmail()
        {
            // Arrange
            _bookLogsDbSet.SetDbSetDataForAsync(new List<BookLog>());

            // Act
            await _sut.RemindAboutBooksAsync(10);

            // Assert
            await _mailingService.DidNotReceive()
                .SendEmailAsync(Arg.Any<EmailDto>());
        }

        [Test]
        public async Task RemindAboutBooksAsync_WithOverdueBook_SendsReminderEmail()
        {
            // Arrange
            var daysBefore = 10;
            var organizationId = 1;
            var userId = "user1";
            var bookOfficeId = 1;
            var officeId = 1;
            var takenDate = DateTime.UtcNow.AddDays(-15);

            var bookLog = new BookLog
            {
                Id = 1,
                ApplicationUserId = userId,
                BookOfficeId = bookOfficeId,
                OrganizationId = organizationId,
                TakenFrom = takenDate,
                Returned = null,
                BookOffice = new BookOffice
                {
                    Id = bookOfficeId,
                    OfficeId = officeId,
                    OrganizationId = organizationId,
                    Book = new Book
                    {
                        Id = 1,
                        Title = "Test Book",
                        Author = "Test Author",
                        OrganizationId = organizationId
                    }
                }
            };

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "user@test.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var organization = new Organization
            {
                Id = organizationId,
                ShortName = "TestOrg"
            };

            var expectedEmailContent = "<html>Book Reminder</html>";

            _bookLogsDbSet.SetDbSetDataForAsync(new List<BookLog> { bookLog });

            _userService.GetApplicationUserOrDefaultAsync(userId)
                .Returns(Task.FromResult(user));

            _organizationService.GetOrganizationByIdAsync(organizationId)
                .Returns(Task.FromResult(organization));

            _appSettings.UserNotificationSettingsUrl("TestOrg")
                .Returns("http://settings.test.com");

            _appSettings.BookUrl("TestOrg", bookOfficeId, officeId)
                .Returns("http://books.test.com/1");

            _mailTemplate.GenerateAsync(
                Arg.Any<BookReminderEmailTemplateViewModel>(),
                Arg.Any<string>())
                .Returns(Task.FromResult(expectedEmailContent));

            // Act
            await _sut.RemindAboutBooksAsync(daysBefore);

            // Assert
            await _mailTemplate.Received(1)
                .GenerateAsync(
                    Arg.Any<BookReminderEmailTemplateViewModel>(),
                    Arg.Is<string>(key => key == EmailPremiumTemplateCacheKeys.BookRemind));

            await _mailingService.Received(1)
                .SendEmailAsync(Arg.Is<EmailDto>(email =>
                    email.Receivers.Contains("user@test.com") &&
                    email.Body == expectedEmailContent));
        }

        [Test]
        public async Task RemindAboutBooksAsync_WithReturnedBook_DoesNotSendReminder()
        {
            // Arrange
            var daysBefore = 10;
            var organizationId = 1;
            var takenDate = DateTime.UtcNow.AddDays(-15);

            var bookLog = new BookLog
            {
                Id = 1,
                ApplicationUserId = "user1",
                BookOfficeId = 1,
                OrganizationId = organizationId,
                TakenFrom = takenDate,
                Returned = DateTime.UtcNow,  // Book is returned
                BookOffice = new BookOffice
                {
                    OfficeId = 1,
                    OrganizationId = organizationId,
                    Book = new Book { Title = "Test", Author = "Author" }
                }
            };

            _bookLogsDbSet.SetDbSetDataForAsync(new List<BookLog> { bookLog });

            // Act
            await _sut.RemindAboutBooksAsync(daysBefore);

            // Assert
            await _mailingService.DidNotReceive()
                .SendEmailAsync(Arg.Any<EmailDto>());
        }

        [Test]
        public async Task RemindAboutBooksAsync_WithNullUser_SkipsAndContinues()
        {
            // Arrange
            var daysBefore = 10;
            var organizationId = 1;
            var takenDate = DateTime.UtcNow.AddDays(-15);

            var bookLog = new BookLog
            {
                Id = 1,
                ApplicationUserId = "nonexistentuser",
                BookOfficeId = 1,
                OrganizationId = organizationId,
                TakenFrom = takenDate,
                Returned = null,
                BookOffice = new BookOffice
                {
                    OfficeId = 1,
                    OrganizationId = organizationId,
                    Book = new Book { Title = "Test", Author = "Author" }
                }
            };

            _bookLogsDbSet.SetDbSetDataForAsync(new List<BookLog> { bookLog });

            _userService.GetApplicationUserOrDefaultAsync("nonexistentuser")
                .Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            await _sut.RemindAboutBooksAsync(daysBefore);

            // Assert
            await _mailingService.DidNotReceive()
                .SendEmailAsync(Arg.Any<EmailDto>());
        }

        [Test]
        public async Task RemindAboutBooksAsync_WithMultipleOverdueBooks_SendsMultipleEmails()
        {
            // Arrange
            var daysBefore = 10;
            var organizationId = 1;
            var takenDate = DateTime.UtcNow.AddDays(-15);

            var bookLog1 = new BookLog
            {
                Id = 1,
                ApplicationUserId = "user1",
                BookOfficeId = 1,
                OrganizationId = organizationId,
                TakenFrom = takenDate,
                Returned = null,
                BookOffice = new BookOffice
                {
                    OfficeId = 1,
                    OrganizationId = organizationId,
                    Book = new Book { Id = 1, Title = "Book 1", Author = "Author 1" }
                }
            };

            var bookLog2 = new BookLog
            {
                Id = 2,
                ApplicationUserId = "user2",
                BookOfficeId = 2,
                OrganizationId = organizationId,
                TakenFrom = takenDate,
                Returned = null,
                BookOffice = new BookOffice
                {
                    OfficeId = 2,
                    OrganizationId = organizationId,
                    Book = new Book { Id = 2, Title = "Book 2", Author = "Author 2" }
                }
            };

            var user1 = new ApplicationUser
            {
                Id = "user1",
                Email = "user1@test.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var user2 = new ApplicationUser
            {
                Id = "user2",
                Email = "user2@test.com",
                FirstName = "Jane",
                LastName = "Smith"
            };

            var organization = new Organization
            {
                Id = organizationId,
                ShortName = "TestOrg"
            };

            _bookLogsDbSet.SetDbSetDataForAsync(new List<BookLog> { bookLog1, bookLog2 });

            _userService.GetApplicationUserOrDefaultAsync("user1")
                .Returns(Task.FromResult(user1));

            _userService.GetApplicationUserOrDefaultAsync("user2")
                .Returns(Task.FromResult(user2));

            _organizationService.GetOrganizationByIdAsync(organizationId)
                .Returns(Task.FromResult(organization));

            _appSettings.UserNotificationSettingsUrl(Arg.Any<string>())
                .Returns("http://settings.test.com");

            _appSettings.BookUrl(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns("http://books.test.com/1");

            _mailTemplate.GenerateAsync(
                Arg.Any<BookReminderEmailTemplateViewModel>(),
                Arg.Any<string>())
                .Returns(Task.FromResult("<html>Reminder</html>"));

            // Act
            await _sut.RemindAboutBooksAsync(daysBefore);

            // Assert
            await _mailTemplate.Received(2)
                .GenerateAsync(
                    Arg.Any<BookReminderEmailTemplateViewModel>(),
                    Arg.Is<string>(key => key == EmailPremiumTemplateCacheKeys.BookRemind));

            await _mailingService.Received(2)
                .SendEmailAsync(Arg.Any<EmailDto>());
        }
    }
}
