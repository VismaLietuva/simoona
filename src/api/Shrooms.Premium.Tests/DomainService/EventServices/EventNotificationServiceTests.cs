using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Events;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.Services.Email.Event;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventNotificationServiceTests
    {
        private IMailingService _mailingService;
        private IApplicationSettings _applicationSettings;
        private IEmailMarkdownRenderer _emailMarkdownRenderer;

        private EventNotificationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _mailingService = Substitute.For<IMailingService>();
            _applicationSettings = Substitute.For<IApplicationSettings>();
            _emailMarkdownRenderer = Substitute.For<IEmailMarkdownRenderer>();

            var uow = Substitute.For<IUnitOfWork2>();
            var mailTemplate = Substitute.For<IMailTemplate>();
            var organizationService = Substitute.For<IOrganizationService>();

            _sut = new EventNotificationService(
                uow,
                mailTemplate,
                _mailingService,
                _applicationSettings,
                organizationService,
                _emailMarkdownRenderer);
        }

        [Test]
        public async Task NotifyNewEventAsync_RendersTheDescriptionCutToTheTeaserLength()
        {
            var userOrg = new UserAndOrganizationHubDto { OrganizationId = 3, OrganizationName = "org", UserId = "u" };
            var description = new string('a', 400) + " :parrot: @[Jonas](user:3f2a9c1e-0000-4000-8000-000000000000)";
            var eventDto = new CreateEventDto { Id = "e1", Name = "Party", Description = description, StartDate = DateTime.UtcNow };

            await _sut.NotifyNewEventAsync(eventDto, new List<IEmailReceiver>(), userOrg);

            await _emailMarkdownRenderer.Received(1).RenderAsync(description, 3, "org", 150);
        }

        [Test]
        public async Task NotifySharedEventAsync_RendersThePostInFullAndTheDescriptionAsATeaser()
        {
            var userOrg = new UserAndOrganizationHubDto { OrganizationId = 3, OrganizationName = "org", UserId = "u" };
            var shared = new SharedEventEmailDto
            {
                CreatedPost = new NewlyCreatedPostDto { Id = 7, MessageBody = "look :parrot:", WallName = "Main", SharedEventId = "e1", User = new UserDto { FullName = "Jonas" } },
                Details = new SharedEventEmailDetailsDto { Name = "Party", Description = "bring :tada:", StartDate = DateTime.UtcNow },
                Receivers = new List<WallMemberEmailReceiverDto>()
            };

            await _sut.NotifySharedEventAsync(shared, userOrg);

            await _emailMarkdownRenderer.Received(1).RenderAsync("look :parrot:", 3, "org", null);
            await _emailMarkdownRenderer.Received(1).RenderAsync("bring :tada:", 3, "org", 150);
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
    }
}
