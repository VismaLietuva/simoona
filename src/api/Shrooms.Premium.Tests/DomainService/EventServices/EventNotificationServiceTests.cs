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
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.Services.Email.Event;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventNotificationServiceTests
    {
        private IMailingService _mailingService;
        private IApplicationSettings _applicationSettings;
        private IMailTemplate _mailTemplate;
        private IUnitOfWork2 _uow;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Notification> _notificationsDbSet;

        private EventNotificationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _mailingService = Substitute.For<IMailingService>();
            _applicationSettings = Substitute.For<IApplicationSettings>();

            _uow = Substitute.For<IUnitOfWork2>();
            _mailTemplate = Substitute.For<IMailTemplate>();
            _usersDbSet = _uow.MockDbSetForAsync<ApplicationUser>();
            _notificationsDbSet = _uow.MockDbSetForAsync<Notification>();

            var organizationService = Substitute.For<IOrganizationService>();
            var markdownConverter = Substitute.For<IMarkdownConverter>();

            organizationService.GetOrganizationByIdAsync(1).Returns(new Organization { Id = 1, ShortName = "name" });
            _applicationSettings.UserNotificationSettingsUrl("name").Returns("settings-url");
            _applicationSettings.EventUrl("name", Arg.Any<string>()).Returns("event-url");

            _sut = new EventNotificationService(
                _uow,
                _mailTemplate,
                _mailingService,
                _applicationSettings,
                organizationService,
                markdownConverter);
        }

        [Test]
        public async Task RemindUsersAboutDeadlineDateOfJoinedEventsAsync_ValidValues_SendsEmails()
        {
            // Arrange
            var organization = new Organization
            {
                Id = 1,
                ShortName = "name"
            };
            var deadlineEmailDtos = new List<EventReminderDeadlineEmailDto>
            {
                new EventReminderDeadlineEmailDto
                {
                    StartDate = DateTime.UtcNow,
                    DeadlineDate = DateTime.UtcNow,
                    Receivers = new List<EventReminderEmailReceiverDto>
                    {
                        new EventReminderEmailReceiverDto
                        {
                            Email = "email@email.com",
                            TimeZoneKey = TimeZoneInfo.Local.Id
                        }
                    },
                    EventId = Guid.NewGuid(),
                    EventName = "name"
                }
            };
            _applicationSettings.UserNotificationSettingsUrl(Arg.Is(organization.ShortName))
                .Returns("url");

            // Act
            await _sut.RemindUsersAboutDeadlineDateOfJoinedEventsAsync(deadlineEmailDtos, organization);

            // Assert
            await _mailingService.Received().SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
        }

        [Test]
        public async Task RemindUsersAboutStartDateOfJoinedEventsAsync_ValidValues_SendsEmails()
        {
            // Arrange
            var organization = new Organization
            {
                Id = 1,
                ShortName = "name"
            };
            var startEmailDtos = new List<EventReminderStartEmailDto>
            {
                new EventReminderStartEmailDto
                {
                    StartDate = DateTime.UtcNow,
                    Receivers = new List<EventReminderEmailReceiverDto>
                    {
                        new EventReminderEmailReceiverDto
                        {
                            Email = "email@email.com",
                            TimeZoneKey = TimeZoneInfo.Local.Id
                        }
                    },
                    EventId = Guid.NewGuid(),
                    EventName = "name"
                }
            };
            _applicationSettings.UserNotificationSettingsUrl(Arg.Is(organization.ShortName))
                .Returns("url");

            // Act
            await _sut.RemindUsersAboutStartDateOfJoinedEventsAsync(startEmailDtos, organization);

            // Assert
            await _mailingService.Received().SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_NoSettings_SendsEmailAndAddsInAppNotification()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var startDate = new DateTime(2026, 3, 17, 9, 30, 0, DateTimeKind.Utc);
            ArrangeUsers(new ApplicationUser { Id = "u1", Email = "u1@email.com", TimeZone = "Europe/Vilnius", NotificationsSettings = null });
            var emails = ArrangeEmailCapture(out var templates);
            Notification notification = null;
            _notificationsDbSet.When(x => x.Add(Arg.Any<Notification>())).Do(call => notification = call.Arg<Notification>());

            // Act
            await _sut.NotifyWaitlistPromotedAsync(eventId, "Museum", startDate, 1, new[] { "u1" });

            // Assert
            var email = emails.Single();
            Assert.That(email.Subject, Is.EqualTo("You're in: Museum"));
            Assert.That(email.Receivers, Is.EqualTo(new[] { "u1@email.com" }));

            var template = templates.Single();
            Assert.That(template.EventTitle, Is.EqualTo("Museum"));
            Assert.That(template.EventUrl, Is.EqualTo("event-url"));
            Assert.That(template.StartDate, Is.EqualTo(startDate));
            Assert.That(template.UserNotificationSettingsUrl, Is.EqualTo("settings-url"));

            _notificationsDbSet.Received(1).Add(Arg.Any<Notification>());
            Assert.That(notification.Type, Is.EqualTo(NotificationType.EventWaitlistPromoted));
            Assert.That(notification.Title, Is.EqualTo("Waitlist"));
            Assert.That(notification.Description, Is.EqualTo("Museum"));
            Assert.That(notification.PictureId, Is.Null);
            Assert.That(notification.OrganizationId, Is.EqualTo(1));
            Assert.That(notification.Sources.EventId, Is.EqualTo(eventId.ToString()));
            Assert.That(notification.NotificationUsers.Single().UserId, Is.EqualTo("u1"));
            Assert.That(notification.Created, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
            Assert.That(notification.Modified, Is.EqualTo(notification.Created));
            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_EmailAndAppDisabled_NotifiesNobody()
        {
            // Arrange
            ArrangeUsers(new ApplicationUser
            {
                Id = "u1",
                Email = "u1@email.com",
                NotificationsSettings = new NotificationsSettings { EventsAppNotifications = false, EventsEmailNotifications = false }
            });

            // Act
            await _sut.NotifyWaitlistPromotedAsync(Guid.NewGuid(), "Museum", DateTime.UtcNow, 1, new[] { "u1" });

            // Assert
            await _mailingService.DidNotReceive().SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
            _notificationsDbSet.DidNotReceive().Add(Arg.Any<Notification>());
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<bool>());
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_OnlyEmailEnabled_SendsEmailWithoutInAppNotification()
        {
            // Arrange
            ArrangeUsers(new ApplicationUser
            {
                Id = "u1",
                Email = "u1@email.com",
                NotificationsSettings = new NotificationsSettings { EventsAppNotifications = false, EventsEmailNotifications = true }
            });

            // Act
            await _sut.NotifyWaitlistPromotedAsync(Guid.NewGuid(), "Museum", DateTime.UtcNow, 1, new[] { "u1" });

            // Assert
            await _mailingService.Received(1).SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
            _notificationsDbSet.DidNotReceive().Add(Arg.Any<Notification>());
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_OnlyAppEnabled_AddsInAppNotificationWithoutEmail()
        {
            // Arrange
            ArrangeUsers(new ApplicationUser
            {
                Id = "u1",
                Email = "u1@email.com",
                NotificationsSettings = new NotificationsSettings { EventsAppNotifications = true, EventsEmailNotifications = false }
            });

            // Act
            await _sut.NotifyWaitlistPromotedAsync(Guid.NewGuid(), "Museum", DateTime.UtcNow, 1, new[] { "u1" });

            // Assert
            await _mailingService.DidNotReceive().SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
            _notificationsDbSet.Received(1).Add(Arg.Is<Notification>(n => n.NotificationUsers.Single().UserId == "u1"));
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_SeveralUsers_AddsOneNotificationForOnlyThePromotedAndOptedIn()
        {
            // Arrange
            ArrangeUsers(
                new ApplicationUser { Id = "u1", Email = "u1@email.com", NotificationsSettings = null },
                new ApplicationUser
                {
                    Id = "u2",
                    Email = "u2@email.com",
                    NotificationsSettings = new NotificationsSettings { EventsAppNotifications = false, EventsEmailNotifications = true }
                },
                new ApplicationUser { Id = "u3", Email = "u3@email.com", NotificationsSettings = null },
                new ApplicationUser { Id = "u4", Email = "u4@email.com", NotificationsSettings = null });
            var emails = ArrangeEmailCapture(out _);

            // Act
            await _sut.NotifyWaitlistPromotedAsync(Guid.NewGuid(), "Museum", DateTime.UtcNow, 1, new[] { "u1", "u2", "u3" });

            // Assert
            Assert.That(emails.SelectMany(e => e.Receivers), Is.EquivalentTo(new[] { "u1@email.com", "u2@email.com", "u3@email.com" }));
            _notificationsDbSet.Received(1).Add(Arg.Is<Notification>(n =>
                n.NotificationUsers.Select(u => u.UserId).OrderBy(id => id).SequenceEqual(new[] { "u1", "u3" })));
        }

        [Test]
        public async Task NotifyWaitlistPromotedAsync_UserWithoutEmail_StillGetsInAppNotification()
        {
            // Arrange
            ArrangeUsers(new ApplicationUser { Id = "u1", Email = null, NotificationsSettings = null });

            // Act
            await _sut.NotifyWaitlistPromotedAsync(Guid.NewGuid(), "Museum", DateTime.UtcNow, 1, new[] { "u1" });

            // Assert
            await _mailingService.DidNotReceive().SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>());
            _notificationsDbSet.Received(1).Add(Arg.Is<Notification>(n => n.NotificationUsers.Single().UserId == "u1"));
        }

        private void ArrangeUsers(params ApplicationUser[] users)
        {
            _usersDbSet.SetDbSetDataForAsync(users);
        }

        private List<EmailDto> ArrangeEmailCapture(out List<EventWaitlistPromotedEmailTemplateViewModel> templates)
        {
            var sentEmails = new List<EmailDto>();
            var capturedTemplates = new List<EventWaitlistPromotedEmailTemplateViewModel>();

            _mailTemplate
                .GenerateAsync(Arg.Do<EventWaitlistPromotedEmailTemplateViewModel>(capturedTemplates.Add), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
                .Returns(callInfo =>
                {
                    var group = Substitute.For<ITimeZoneEmailGroup>();
                    group.Values.Returns(callInfo.Arg<IEnumerable<string>>().ToDictionary(key => key, _ => "body"));
                    return group;
                });
            _mailingService
                .When(service => service.SendEmailsAsync(Arg.Any<IEnumerable<EmailDto>>()))
                .Do(callInfo => sentEmails.AddRange(callInfo.Arg<IEnumerable<EmailDto>>().ToList()));

            templates = capturedTemplates;
            return sentEmails;
        }
    }
}
