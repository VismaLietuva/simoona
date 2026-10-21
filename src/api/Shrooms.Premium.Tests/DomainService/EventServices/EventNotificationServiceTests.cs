using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Emoji;
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
        private IMarkdownConverter _markdownConverter;
        private ICustomEmojiService _customEmojiService;

        private EventNotificationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _mailingService = Substitute.For<IMailingService>();
            _applicationSettings = Substitute.For<IApplicationSettings>();
            _markdownConverter = Substitute.For<IMarkdownConverter>();
            _customEmojiService = Substitute.For<ICustomEmojiService>();

            var uow = Substitute.For<IUnitOfWork2>();
            var mailTemplate = Substitute.For<IMailTemplate>();
            var organizationService = Substitute.For<IOrganizationService>();

            _sut = new EventNotificationService(
                uow,
                mailTemplate,
                _mailingService,
                _applicationSettings,
                organizationService,
                _markdownConverter,
                _customEmojiService);
        }

        [Test]
        public async Task NotifyNewEventAsync_PassesTheOrganisationsEmojiToTheConverter()
        {
            var emojis = new CustomEmojiListDto
            {
                Emojis = new List<CustomEmojiDto> { new CustomEmojiDto { Name = "parrot", Url = "/storage/org/parrot.gif" } }
            };
            _customEmojiService.GetAllAsync(3, "org").Returns(emojis);
            _applicationSettings.ApiUrl.Returns("https://api.example.com");
            var userOrg = new UserAndOrganizationHubDto { OrganizationId = 3, OrganizationName = "org", UserId = "u" };
            var eventDto = new CreateEventDto { Id = "e1", Name = "Party", Description = "bring :parrot:", StartDate = DateTime.UtcNow };

            await _sut.NotifyNewEventAsync(eventDto, new List<IEmailReceiver>(), userOrg);

            _markdownConverter.Received(1).ConvertToHtml("bring :parrot:", emojis.Emojis, "https://api.example.com");
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
