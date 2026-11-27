using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.Email.ServiceRequest;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class ServiceRequestNotificationServiceTests
    {
        private const int OrganizationId = 3;
        private const string OrganizationShortName = "org";
        private const string Markdown = "**On** the way";
        private const string Html = "<p><strong>On</strong> the way</p>";

        [Test]
        public async Task NotifyAboutNewCommentAsync_RendersCommentAsMarkdown()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            uow.MockDbSetForAsync(new List<Organization> { new Organization { Id = OrganizationId, ShortName = OrganizationShortName, Name = "Org" } });
            uow.MockDbSetForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "requester", Email = "requester@example.com", FirstName = "Tomas", LastName = "Kazlauskas" },
                new ApplicationUser { Id = "commenter", Email = "commenter@example.com", FirstName = "Rasa", LastName = "Petraitiene" }
            });
            uow.MockDbSetForAsync(new List<ApplicationRole> { new ApplicationRole { Id = "role", Name = Roles.ServiceRequestNotification } });
            uow.MockDbSetForAsync(new List<IdentityUserRole<string>>());
            uow.MockDbSetForAsync(new List<ServiceRequestStatus>());
            uow.MockDbSetForAsync(new List<ServiceRequest> { new ServiceRequest { Id = 7, OrganizationId = OrganizationId, EmployeeId = "requester", Title = "Broken monitor" } });
            var mailTemplate = Substitute.For<IMailTemplate>();
            var emailMarkdownRenderer = Substitute.For<IEmailMarkdownRenderer>();
            emailMarkdownRenderer.RenderAsync(Markdown, OrganizationId, OrganizationShortName).Returns(Html);
            var sut = new ServiceRequestNotificationService(
                uow,
                Substitute.For<IMailingService>(),
                mailTemplate,
                Substitute.For<IApplicationSettings>(),
                emailMarkdownRenderer);

            await sut.NotifyAboutNewCommentAsync(new ServiceRequestCreatedCommentDto
            {
                ServiceRequestId = 7,
                CommentedEmployeeId = "commenter",
                CommentContent = Markdown
            });

            await mailTemplate.Received(1).GenerateAsync(
                Arg.Is<ServiceRequestCommentEmailTemplateViewModel>(model => model.Comment == Html),
                EmailPremiumTemplateCacheKeys.ServiceRequestComment);
        }
    }
}
