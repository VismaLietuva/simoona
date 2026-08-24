using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Support;
using Shrooms.Tests.Extensions;
using MailAttachment = System.Net.Mail.Attachment;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class SupportServiceTests
    {
        private const string UserId = "user-1";

        private IUnitOfWork2 _uow;
        private DbSet<ApplicationUser> _usersDbSet;
        private IMailingService _mailingService;
        private IApplicationSettings _applicationSettings;
        private ISupportService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = UserId, FirstName = "Ann", LastName = "Reporter", Email = "ann@test.com" }
            });

            _mailingService = Substitute.For<IMailingService>();
            _applicationSettings = Substitute.For<IApplicationSettings>();
            _applicationSettings.SupportEmail.Returns("support@test.com");

            _sut = new SupportService(_uow, _mailingService, _applicationSettings);
        }

        [Test]
        public async Task SubmitTicketAsync_WithoutAttachments_SendsEmailWithNoAttachments()
        {
            var sent = CaptureSentEmail();

            await _sut.SubmitTicketAsync(UserAndOrganization(), Ticket());

            Assert.That(sent.Attachments, Is.Empty);
        }

        [Test]
        public async Task SubmitTicketAsync_WithSeveralAttachments_SendsThemAllOnOneEmail()
        {
            var ticket = Ticket(Attachment("one.png"), Attachment("two.png"), Attachment("three.png"));
            var sent = CaptureSentEmail();

            await _sut.SubmitTicketAsync(UserAndOrganization(), ticket);

            await _mailingService.Received(1).SendEmailAsync(Arg.Any<EmailDto>(), true);
            Assert.That(sent.Names, Is.EqualTo(new[] { "one.png", "two.png", "three.png" }));
        }

        [Test]
        public async Task SubmitTicketAsync_WithSeveralAttachments_KeepsContentReadableUntilSent()
        {
            var ticket = Ticket(Attachment("one.png", "first"), Attachment("two.png", "second"));
            var sent = CaptureSentEmail();

            await _sut.SubmitTicketAsync(UserAndOrganization(), ticket);

            Assert.That(sent.Contents, Is.EqualTo(new[] { "first", "second" }));
        }

        // The streams are held open for the send, so every one has to be released
        // afterwards rather than just the last.
        [Test]
        public async Task SubmitTicketAsync_WithSeveralAttachments_DisposesEveryAttachment()
        {
            var ticket = Ticket(Attachment("one.png"), Attachment("two.png"));
            var sent = CaptureSentEmail();

            await _sut.SubmitTicketAsync(UserAndOrganization(), ticket);

            foreach (var attachment in sent.Attachments)
            {
                Assert.Throws<ObjectDisposedException>(() => _ = attachment.ContentStream.Length);
            }
        }

        [Test]
        public async Task SubmitTicketAsync_WhenSendingThrows_StillDisposesEveryAttachment()
        {
            var ticket = Ticket(Attachment("one.png"), Attachment("two.png"));
            var sent = CaptureSentEmail(new InvalidOperationException("smtp down"));

            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SubmitTicketAsync(UserAndOrganization(), ticket));

            Assert.That(sent.Attachments, Has.Count.EqualTo(2));
            foreach (var attachment in sent.Attachments)
            {
                Assert.Throws<ObjectDisposedException>(() => _ = attachment.ContentStream.Length);
            }
        }

        private static UserAndOrganizationDto UserAndOrganization() =>
            new UserAndOrganizationDto { UserId = UserId, OrganizationId = 1 };

        private static SupportDto Ticket(params SupportAttachmentDto[] attachments) =>
            new SupportDto
            {
                Subject = "Broken flow",
                Message = "Steps attached",
                Type = SupportType.Bug,
                Attachments = attachments.ToList()
            };

        private static SupportAttachmentDto Attachment(string fileName, string content = "body") =>
            new SupportAttachmentDto
            {
                Content = Encoding.UTF8.GetBytes(content),
                FileName = fileName,
                ContentType = "image/png"
            };

        // The service disposes its attachments before returning, so what the mail
        // carried has to be read inside the send itself.
        private SentEmail CaptureSentEmail(Exception throwInstead = null)
        {
            var sent = new SentEmail();

            _mailingService.SendEmailAsync(Arg.Do<EmailDto>(email =>
            {
                sent.Attachments = email.Attachments.ToList();
                sent.Names = email.Attachments.Select(attachment => attachment.Name).ToList();
                sent.Contents = email.Attachments.Select(ReadContent).ToList();
            }), true).Returns(_ => throwInstead != null ? Task.FromException(throwInstead) : Task.CompletedTask);

            return sent;
        }

        private static string ReadContent(MailAttachment attachment)
        {
            using var reader = new StreamReader(attachment.ContentStream, Encoding.UTF8, false, 1024, true);
            var content = reader.ReadToEnd();
            attachment.ContentStream.Position = 0;
            return content;
        }

        private sealed class SentEmail
        {
            public IList<MailAttachment> Attachments { get; set; } = new List<MailAttachment>();

            public IList<string> Names { get; set; } = new List<string>();

            public IList<string> Contents { get; set; } = new List<string>();
        }
    }
}
