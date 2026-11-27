using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Email;
using Shrooms.Domain.Services.Email.Kudos;
using Shrooms.Tests.Extensions;
using System.Linq;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class KudosNotificationServiceTests
    {
        private const int OrganizationId = 3;
        private const string OrganizationShortName = "org";
        private const string Markdown = "**Great**";
        private const string Html = "<p><strong>Great</strong></p>";

        private IMailTemplate _mailTemplate;
        private IEmailMarkdownRenderer _emailMarkdownRenderer;
        private KudosNotificationService _sut;

        [SetUp]
        public void SetUp()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            uow.MockDbSetForAsync(new List<Organization> { new Organization { Id = OrganizationId, ShortName = OrganizationShortName } });
            uow.MockDbSetForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "sender", FirstName = "Jane", LastName = "Doe", Email = "sender@example.com" },
                new ApplicationUser { Id = "receiver", FirstName = "Ada", LastName = "Lovelace", Email = "receiver@example.com" }
            });

            _mailTemplate = Substitute.For<IMailTemplate>();
            _emailMarkdownRenderer = Substitute.For<IEmailMarkdownRenderer>();
            _emailMarkdownRenderer.RenderAsync(Markdown, OrganizationId, OrganizationShortName).Returns(Html);

            var permissionService = Substitute.For<IPermissionService>();
            permissionService.FilterEmailsByPermissionAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>())
                .Returns(call => Task.FromResult<IList<string>>(call.Arg<IEnumerable<string>>().ToList()));

            _sut = new KudosNotificationService(
                uow,
                Substitute.For<IMailingService>(),
                Substitute.For<IApplicationSettings>(),
                _mailTemplate,
                _emailMarkdownRenderer,
                permissionService);
        }

        [Test]
        public async Task NotifyApprovedKudosRecipientAsync_RendersCommentAsMarkdown()
        {
            await _sut.NotifyApprovedKudosRecipientAsync(KudosLog());

            await _mailTemplate.Received(1).GenerateAsync(
                Arg.Is<KudosReceivedDecreasedEmailTemplateViewModel>(model => model.Comment == Html),
                EmailTemplateCacheKeys.KudosReceived);
        }

        [Test]
        public async Task NotifyApprovedKudosDecreaseRecipientAsync_RendersCommentAsMarkdown()
        {
            await _sut.NotifyApprovedKudosDecreaseRecipientAsync(KudosLog());

            await _mailTemplate.Received(1).GenerateAsync(
                Arg.Is<KudosReceivedDecreasedEmailTemplateViewModel>(model => model.Comment == Html),
                EmailTemplateCacheKeys.KudosDecreased);
        }

        [Test]
        public async Task NotifyRejectedKudosLogSenderAsync_RendersCommentAsMarkdown()
        {
            await _sut.NotifyRejectedKudosLogSenderAsync(KudosLog());

            await _mailTemplate.Received(1).GenerateAsync(
                Arg.Is<KudosRejectedEmailTemplateViewModel>(model => model.Comment == Html),
                EmailTemplateCacheKeys.KudosRejected);
        }

        [Test]
        public async Task NotifyAboutKudosSentAsync_RendersCommentAsMarkdown()
        {
            var kudosDto = new AddKudosDto
            {
                KudosLog = new AddKudosLogDto { Comment = Markdown, OrganizationId = OrganizationId },
                ReceivingUser = new ApplicationUserDto { Id = "receiver" },
                SendingUser = new ApplicationUserDto { Id = "sender", FirstName = "Jane", LastName = "Doe" },
                TotalKudosPointsInLog = 5
            };

            await _sut.NotifyAboutKudosSentAsync(kudosDto);

            await _mailTemplate.Received(1).GenerateAsync(
                Arg.Is<KudosSentEmailTemplateViewModel>(model => model.Comment == Html),
                EmailTemplateCacheKeys.KudosSent);
        }

        private static KudosLog KudosLog()
        {
            return new KudosLog
            {
                OrganizationId = OrganizationId,
                CreatedBy = "sender",
                EmployeeId = "receiver",
                Employee = new ApplicationUser { Id = "receiver", FirstName = "Ada", LastName = "Lovelace", Email = "receiver@example.com" },
                Points = 5,
                KudosTypeName = "Send",
                Comments = Markdown
            };
        }
    }
}
