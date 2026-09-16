using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.Calendar;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class EventCalendarServiceTests
    {
        private const int OrganizationId = 1;
        private const string UserId = "user-1";
        private const string OtherUserId = "user-2";

        private IUnitOfWork2 _uow;
        private IMailingService _mailingService;
        private IApplicationSettings _appSettings;

        private DbSet<Event> _eventsDbSet;
        private DbSet<Organization> _organizationsDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;

        private EventCalendarService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _mailingService = Substitute.For<IMailingService>();
            _appSettings = Substitute.For<IApplicationSettings>();

            _uow.MockDbSetForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = UserId, Email = "user1@shrooms.com" },
                new ApplicationUser { Id = OtherUserId, Email = "user2@shrooms.com" }
            });

            _eventsDbSet = _uow.MockDbSetForAsync(new List<Event>());
            _organizationsDbSet = _uow.MockDbSetForAsync(new List<Organization>());
            _eventParticipantsDbSet = _uow.MockDbSetForAsync(new List<EventParticipant>());

            _organizationsDbSet.FindAsync(OrganizationId).Returns(new Organization { Id = OrganizationId, ShortName = "visma" });
            _appSettings.EventUrl(Arg.Any<string>(), Arg.Any<string>()).Returns("https://simoona/event");

            _sut = new EventCalendarService(_uow, _mailingService, _appSettings, Substitute.For<IEventValidationService>());
        }

        [Test]
        public async Task SendInvitationAsync_WhenParticipantChoseOptions_DescriptionContainsChoices()
        {
            var @event = CreateJoinValidationDto(
                CreateOption(1, "KFC Bowl", order: 2),
                CreateOption(2, "Salad", order: 1));

            var description = await CaptureInvitationDescriptionAsync(@event);

            Assert.That(description, Does.Contain("Your choices: Salad, KFC Bowl"));
        }

        [Test]
        public async Task SendInvitationAsync_WhenParticipantChoseOptions_DescriptionStillContainsEventUrl()
        {
            var @event = CreateJoinValidationDto(CreateOption(1, "KFC Bowl"));

            var description = await CaptureInvitationDescriptionAsync(@event);

            Assert.That(description, Does.Contain("https://simoona/event"));
        }

        [Test]
        public async Task SendInvitationAsync_WhenNoOptionsChosen_DescriptionContainsNoChoices()
        {
            var @event = CreateJoinValidationDto();

            var description = await CaptureInvitationDescriptionAsync(@event);

            Assert.That(description, Does.Not.Contain("Your choices"));
        }

        [Test]
        public async Task SendInvitationAsync_WhenChosenOptionBelongsToQuestion_DescriptionIncludesIt()
        {
            var questionOption = CreateOption(2, "Beef", order: 1);
            questionOption.QuestionId = 7;

            var @event = CreateJoinValidationDto(questionOption);

            var description = await CaptureInvitationDescriptionAsync(@event);

            Assert.That(description, Does.Contain("Your choices: Beef"));
        }

        [Test]
        public async Task SendInvitationAsync_WhenParticipantChoseOptions_SummaryCarriesChoices()
        {
            var @event = CreateJoinValidationDto(
                CreateOption(1, "KFC Bowl", order: 2),
                CreateOption(2, "Salad", order: 1));

            var summary = await CaptureInvitationSummaryAsync(@event);

            Assert.That(summary, Is.EqualTo("Asian St. — Salad, KFC Bowl"));
        }

        [Test]
        public async Task SendInvitationAsync_WhenNoOptionsChosen_SummaryIsTheEventName()
        {
            var summary = await CaptureInvitationSummaryAsync(CreateJoinValidationDto());

            Assert.That(summary, Is.EqualTo("Asian St."));
        }

        [Test]
        public async Task DownloadEventAsync_WhenUserChoseOptions_SummaryCarriesChoices()
        {
            var eventId = ArrangeStoredEvent();

            ArrangeParticipants(CreateParticipant(eventId, UserId, CreateOption(1, "KFC Bowl")));

            var export = await _sut.DownloadEventAsync(eventId, OrganizationId, UserId);

            Assert.That(SummaryOf(Encoding.UTF8.GetString(export.Content)), Is.EqualTo("Asian St. — KFC Bowl"));
        }

        [Test]
        public async Task DownloadEventAsync_WhenChosenOptionBelongsToQuestion_DescriptionIncludesIt()
        {
            var eventId = ArrangeStoredEvent();
            var questionOption = CreateOption(2, "Beef");
            questionOption.QuestionId = 7;

            ArrangeParticipants(CreateParticipant(eventId, UserId, questionOption));

            var export = await _sut.DownloadEventAsync(eventId, OrganizationId, UserId);

            Assert.That(DescriptionOf(Encoding.UTF8.GetString(export.Content)), Does.Contain("Your choices: Beef"));
        }

        [Test]
        public async Task DownloadEventAsync_WhenUserChoseOptions_DescriptionContainsOnlyTheirChoices()
        {
            var eventId = ArrangeStoredEvent();

            ArrangeParticipants(
                CreateParticipant(eventId, UserId, CreateOption(1, "KFC Bowl")),
                CreateParticipant(eventId, OtherUserId, CreateOption(2, "Pizza")));

            var export = await _sut.DownloadEventAsync(eventId, OrganizationId, UserId);

            var description = DescriptionOf(Encoding.UTF8.GetString(export.Content));

            Assert.That(description, Does.Contain("Your choices: KFC Bowl"));
            Assert.That(description, Does.Not.Contain("Pizza"));
        }

        [Test]
        public async Task DownloadEventAsync_WhenUserIsNotParticipant_DescriptionContainsNoChoices()
        {
            var eventId = ArrangeStoredEvent();

            ArrangeParticipants(CreateParticipant(eventId, OtherUserId, CreateOption(2, "Pizza")));

            var export = await _sut.DownloadEventAsync(eventId, OrganizationId, UserId);

            var description = DescriptionOf(Encoding.UTF8.GetString(export.Content));

            Assert.That(description, Does.Not.Contain("Your choices"));
        }

        private Guid ArrangeStoredEvent()
        {
            var eventId = Guid.NewGuid();
            var storedEvent = new Event
            {
                Id = eventId,
                Name = "Asian St.",
                Description = "Lunch",
                Place = "6th floor kitchen",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1)
            };

            _eventsDbSet.FindAsync(eventId).Returns(storedEvent);

            return eventId;
        }

        private void ArrangeParticipants(params EventParticipant[] participants)
        {
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants);
        }

        private async Task<string> CaptureInvitationDescriptionAsync(EventJoinValidationDto @event)
        {
            return DescriptionOf(await CaptureInvitationAsync(@event));
        }

        private async Task<string> CaptureInvitationSummaryAsync(EventJoinValidationDto @event)
        {
            return SummaryOf(await CaptureInvitationAsync(@event));
        }

        private async Task<string> CaptureInvitationAsync(EventJoinValidationDto @event)
        {
            string capturedCalendar = null;

            _mailingService
                .SendEmailAsync(Arg.Do<EmailDto>(email => capturedCalendar = ReadAttachment(email)), Arg.Any<bool>())
                .Returns(Task.CompletedTask);

            await _sut.SendInvitationAsync(@event, new List<string> { UserId }, OrganizationId);

            Assert.That(capturedCalendar, Is.Not.Null, "No invitation was sent.");

            return capturedCalendar;
        }

        private static EventJoinValidationDto CreateJoinValidationDto(params EventOption[] selectedOptions)
        {
            return new EventJoinValidationDto
            {
                Id = Guid.NewGuid(),
                Name = "Asian St.",
                Description = "Lunch",
                Location = "6th floor kitchen",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddHours(1),
                Options = selectedOptions.ToList(),
                SelectedOptions = selectedOptions.ToList()
            };
        }

        private static EventOption CreateOption(int id, string name, int order = 0)
        {
            return new EventOption { Id = id, Option = name, Order = order };
        }

        private static EventParticipant CreateParticipant(Guid eventId, string userId, params EventOption[] options)
        {
            return new EventParticipant
            {
                EventId = eventId,
                ApplicationUserId = userId,
                EventOptions = options.ToList()
            };
        }

        private static string ReadAttachment(EmailDto email)
        {
            var attachment = email.Attachments.Single();

            attachment.ContentStream.Position = 0;

            using (var reader = new StreamReader(attachment.ContentStream, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                return reader.ReadToEnd();
            }
        }

        private static string DescriptionOf(string calendar)
        {
            return PropertyOf(calendar, "DESCRIPTION");
        }

        private static string SummaryOf(string calendar)
        {
            return PropertyOf(calendar, "SUMMARY");
        }

        private static string PropertyOf(string calendar, string property)
        {
            // Ical.Net folds a long property onto continuation lines, each introduced by CRLF
            // and a space, and escapes commas, semicolons and newlines in the value. Both are
            // undone here so assertions read like the text a calendar client shows.
            var foldMarker = new string(new[] { (char)13, (char)10, ' ' });
            var unfolded = calendar.Replace(foldMarker, string.Empty);
            var match = Regex.Match(unfolded, $"^{property}:(?<value>.*)$", RegexOptions.Multiline);

            Assert.That(match.Success, Is.True, $"Calendar has no {property} property.");

            return Regex.Unescape(match.Groups["value"].Value.TrimEnd((char)13));
        }
    }
}
