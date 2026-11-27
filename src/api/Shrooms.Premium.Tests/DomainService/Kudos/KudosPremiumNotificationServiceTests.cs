using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;
using Shrooms.Premium.Domain.Services.Email.Kudos;
using Shrooms.Tests.Extensions;
using System.Linq;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Premium.Tests.DomainService.Kudos
{
    [TestFixture]
    public class KudosPremiumNotificationServiceTests
    {
        private const int OrganizationId = 3;
        private const string OrganizationShortName = "org";
        private const string Markdown = "**Two** years";
        private const string Html = "<p><strong>Two</strong> years</p>";

        [Test]
        public async Task SendLoyaltyBotNotificationAsync_RendersCommentAsMarkdown()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            uow.MockDbSetForAsync(new List<Organization> { new Organization { Id = OrganizationId, ShortName = OrganizationShortName, Name = "Org" } });
            uow.MockDbSetForAsync(new List<ApplicationUser> { new ApplicationUser { Id = "employee", Email = "employee@example.com" } });
            var mailTemplate = Substitute.For<IMailTemplate>();
            var emailMarkdownRenderer = Substitute.For<IEmailMarkdownRenderer>();
            emailMarkdownRenderer.RenderAsync(Markdown, OrganizationId, OrganizationShortName).Returns(Html);
            var permissionService = Substitute.For<IPermissionService>();
            permissionService.FilterEmailsByPermissionAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<string>())
                .Returns(call => Task.FromResult<IList<string>>(call.Arg<IEnumerable<string>>().ToList()));

            var sut = new KudosPremiumNotificationService(
                uow,
                Substitute.For<IMailingService>(),
                Substitute.For<IApplicationSettings>(),
                mailTemplate,
                emailMarkdownRenderer,
                permissionService);

            await sut.SendLoyaltyBotNotificationAsync(new List<AwardedKudosEmployeeDto>
            {
                new AwardedKudosEmployeeDto { EmployeeId = "employee", OrganizationId = OrganizationId, Points = 5, KudosTypeName = "Loyalty", Comments = Markdown }
            });

            await mailTemplate.Received(1).GenerateAsync(
                Arg.Is<LoyaltyKudosReceivedDecreasedEmailTemplateViewModel>(model => model.Comment == Html),
                EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived);
        }
    }
}
