using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.Domain.Services.Email.Vacations;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationNotificationServiceTests
    {
        private const string Employee = "employee-id";
        private const string Manager = "manager-id";
        private const string Admin = "admin-id";

        private IUnitOfWork2 _uow;
        private IMailingService _mailingService;
        private DbSet<ApplicationUser> _userDbSet;
        private IMailTemplate _mailTemplate;
        private DbSet<Notification> _notificationDbSet;

        private VacationNotificationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _mailingService = Substitute.For<IMailingService>();

            _mailTemplate = Substitute.For<IMailTemplate>();

            var appSettings = Substitute.For<IApplicationSettings>();
            appSettings
                .VacationApproveUrl(Arg.Any<string>(), Arg.Any<int>())
                .Returns(call => $"approve/{call.ArgAt<int>(1)}");

            _userDbSet = _uow.MockDbSetForAsync<ApplicationUser>();
            _notificationDbSet = _uow.MockDbSetForAsync<Notification>();
            _uow.MockDbSetForAsync(new List<Organization>
            {
                new Organization { Id = 1, ShortName = "visma" }
            });

            _sut = new VacationNotificationService(_uow, _mailingService, appSettings, _mailTemplate);
        }

        [Test]
        public async Task NotifySubmittedAsync_EmployeeSubmits_EmailsTheManagerOnly()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifySubmittedAsync(Request(), Actor(Employee));

            Assert.That(Mailed(), Is.EqualTo(new[] { Email(Manager) }));
        }

        [Test]
        public async Task NotifySubmittedAsync_ManagerIsTheEmployee_MailsNobody()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true, managerId: Employee));

            await _sut.NotifySubmittedAsync(Request(), Actor(Employee));

            Assert.That(Mailed(), Is.Empty);
        }

        [Test]
        public async Task NotifyDecidedAsync_ManagerDecides_EmailsTheEmployeeButNotTheDecider()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "approved"), Actor(Manager));

            Assert.That(Mailed(), Is.EqualTo(new[] { Email(Employee) }));
        }

        [Test]
        public async Task NotifyDecidedAsync_AdminDecides_EmailsBothTheEmployeeAndTheManager()
        {
            GivenPeople(
                WithSettings(Employee, appNotifications: true),
                WithSettings(Manager, appNotifications: true),
                WithSettings(Admin, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "rejected"), Actor(Admin));

            Assert.That(Mailed(), Is.EquivalentTo(new[] { Email(Employee), Email(Manager) }));
        }

        [Test]
        public async Task NotifyDecidedAsync_RecipientSwitchedOffInAppNotifications_StillEmailsThem()
        {
            GivenPeople(WithSettings(Employee, appNotifications: false), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "approved"), Actor(Manager));

            Assert.That(Mailed(), Is.EqualTo(new[] { Email(Employee) }));
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<string>());
        }

        [Test]
        public async Task NotifyDecidedAsync_RecipientKeptInAppNotifications_SavesAnInAppNotification()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "approved"), Actor(Manager));

            await _uow.Received(1).SaveChangesAsync(Arg.Any<string>());
        }

        [Test]
        public async Task NotifyWithdrawnAsync_RecipientHasNoSettingsRow_EmailsAndNotifiesInApp()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), NoSettings(Manager));

            await _sut.NotifyWithdrawnAsync(Request(status: "cancelled"), Actor(Employee));

            Assert.That(Mailed(), Is.EqualTo(new[] { Email(Manager) }));
            await _uow.Received(1).SaveChangesAsync(Arg.Any<string>());
        }

        [Test]
        public async Task NotifyChangedAsync_RecipientHasNoEmailAddress_SkipsTheEmailButStillNotifiesInApp()
        {
            var manager = WithSettings(Manager, appNotifications: true);
            manager.Email = null;

            GivenPeople(WithSettings(Employee, appNotifications: true), manager);

            await _sut.NotifyChangedAsync(Request(), Actor(Employee));

            Assert.That(Mailed(), Is.Empty);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<string>());
        }

        private void GivenPeople(params ApplicationUser[] users)
        {
            _userDbSet.SetDbSetDataForAsync(users.ToList());
        }

        private static ApplicationUser WithSettings(string id, bool appNotifications, string managerId = Manager)
        {
            var user = NoSettings(id, managerId);
            user.NotificationsSettings = new NotificationsSettings { VacationsAppNotifications = appNotifications };
            return user;
        }

        private static ApplicationUser NoSettings(string id, string managerId = Manager)
        {
            return new ApplicationUser
            {
                Id = id,
                FirstName = id,
                LastName = "Person",
                Email = Email(id),
                ManagerId = id == Employee ? managerId : null
            };
        }

        private static string Email(string id) => $"{id}@visma.com";

        private static UserAndOrganizationDto Actor(string userId)
        {
            return new UserAndOrganizationDto { UserId = userId, OrganizationId = 1 };
        }

        private static VacationRequestDto Request(string status = "pending")
        {
            return new VacationRequestDto
            {
                Id = 7,
                Type = "annual",
                Status = status,
                Employee = new VacationPersonDto { Id = Employee, FirstName = "Ada", LastName = "Lovelace" },
                DateFrom = "2026-09-01",
                DateTo = "2026-09-05",
                WorkingDays = 5
            };
        }

        [Test]
        public async Task NotifyChangedAsync_EmployeeEditedTheirOwnRequest_GivesTheManagerTheDecisionButtons()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyChangedAsync(Request(), Actor(Employee));

            Assert.That(Rendered().Single().ApproveUrl, Is.EqualTo("approve/7"));
        }

        [Test]
        public async Task NotifyChangedAsync_ManagerEditedIt_LeavesTheButtonsOffTheEmployeeCopy()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyChangedAsync(Request(), Actor(Manager));

            Assert.That(Rendered().Single().ApproveUrl, Is.Null);
        }

        [Test]
        public async Task NotifyChangedAsync_RequestIsNoLongerPending_LeavesTheButtonsOff()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyChangedAsync(Request(status: "approved"), Actor(Employee));

            Assert.That(Rendered().Single().ApproveUrl, Is.Null);
        }

        [Test]
        public async Task NotifyDecidedAsync_BothSidesGetIt_HeadsEachRowForItsReader()
        {
            GivenPeople(
                WithSettings(Employee, appNotifications: true),
                WithSettings(Manager, appNotifications: true),
                WithSettings(Admin, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "approved"), Actor(Admin));

            var rows = Rows();
            Assert.Multiple(() =>
            {
                Assert.That(Row(rows, NotificationType.VacationRequest).Title, Is.EqualTo("Vacation"));
                Assert.That(Row(rows, NotificationType.VacationReview).Title, Is.EqualTo("Ada Lovelace"));
            });
        }

        [Test]
        public async Task NotifyDecidedAsync_OnlyTheEmployeeIsTold_WritesNoReviewerRow()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "approved"), Actor(Manager));

            Assert.That(Rows().Select(row => row.Type), Is.EqualTo(new[] { NotificationType.VacationRequest }));
        }

        [Test]
        public async Task NotifyDecidedAsync_ARowIsWritten_SaysTheOutcomeInEnglishAndCarriesNoAvatar()
        {
            GivenPeople(WithSettings(Employee, appNotifications: true), WithSettings(Manager, appNotifications: true));

            await _sut.NotifyDecidedAsync(Request(status: "rejected"), Actor(Manager));

            var row = Rows().Single();
            Assert.Multiple(() =>
            {
                Assert.That(row.Description, Is.EqualTo("2026-09-01 – 2026-09-05 — rejected"));
                Assert.That(row.PictureId, Is.Null);
            });
        }

        private Notification[] Rows()
        {
            return _notificationDbSet
                .ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(DbSet<Notification>.Add))
                .Select(call => (Notification)call.GetArguments()[0])
                .ToArray();
        }

        private static Notification Row(Notification[] rows, NotificationType type)
        {
            return rows.Single(row => row.Type == type);
        }

        private VacationSubmittedEmailTemplateViewModel[] Rendered()
        {
            return _mailTemplate
                .ReceivedCalls()
                .Select(call => call.GetArguments()[0])
                .OfType<VacationSubmittedEmailTemplateViewModel>()
                .ToArray();
        }

        private string[] Mailed()
        {
            return _mailingService
                .ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(IMailingService.SendEmailAsync))
                .SelectMany(call => ((EmailDto)call.GetArguments()[0]).Receivers)
                .ToArray();
        }
    }
}
