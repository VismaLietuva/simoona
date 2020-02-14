using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Resources.Emails;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class PostNotificationService : IPostNotificationService
    {
        private readonly IUserService _userService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IMarkdownConverter _markdownConverter;

        private readonly IDbSet<DataLayer.EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;

        public PostNotificationService(
            IUnitOfWork2 uow,
            IUserService userService,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService,
            IMarkdownConverter markdownConverter)
        {
            _appSettings = appSettings;
            _userService = userService;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;

            _wallsDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
        }

        public void NotifyAboutNewPost(Post post, ApplicationUser postCreator)
        {
            var organization = _organizationService.GetOrganizationById(postCreator.OrganizationId);
            var wall = _wallsDbSet.Single(w => w.Id == post.WallId);

            var destinationEmails = _userService.GetWallUsersEmails(postCreator.Email, wall);
            var postLink = GetPostLink(wall.Type, wall.Id, organization.ShortName, post.Id);
            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, postCreator.PictureId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var subject = string.Format(Templates.NewWallPostEmailSubject, wall.Name, postCreator.FullName);
            var body = _markdownConverter.ConvertToHtml(post.MessageBody);

            var emailTemplateViewModel = new NewWallPostEmailTemplateViewModel(
                GetWallTitle(wall),
                authorPictureUrl,
                postCreator.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                GetActionButtonTitle(wall));
            var content = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.NewWallPost);

            var emailData = new EmailDto(destinationEmails, subject, content);
            _mailingService.SendEmail(emailData);
        }

        public void NotifyAboutNewPost(NewlyCreatedPostDTO post)
        {
            var postCreator = _userService.GetApplicationUser(post.User.UserId);

            var organization = _organizationService.GetOrganizationById(postCreator.OrganizationId);
            var wall = _wallsDbSet.Single(w => w.Id == post.WallId);

            var destinationEmails = _userService.GetWallUsersEmails(postCreator.Email, wall);
            var postLink = GetPostLink(post.WallType, post.WallId, organization.ShortName, post.Id);
            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, postCreator.PictureId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var subject = string.Format(Templates.NewWallPostEmailSubject, wall.Name, postCreator.FullName);
            var body = _markdownConverter.ConvertToHtml(post.MessageBody);

            var emailTemplateViewModel = new NewWallPostEmailTemplateViewModel(
                GetWallTitle(wall),
                authorPictureUrl,
                postCreator.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                GetActionButtonTitle(wall));
            var content = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.NewWallPost);

            var emailData = new EmailDto(destinationEmails, subject, content);
            _mailingService.SendEmail(emailData);
        }

        private static string GetActionButtonTitle(DataLayer.EntityModels.Models.Multiwall.Wall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return EmailTemplates.EventActionButtonTitle;
                case WallType.Project:
                    return EmailTemplates.ProjectActionButtonTitle;
                default:
                    return EmailTemplates.DefaultActionButtonTitle;
            }
        }

        private static string GetWallTitle(DataLayer.EntityModels.Models.Multiwall.Wall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return string.Format(EmailTemplates.EventPostTitle, wall.Name);
                case WallType.Project:
                    return string.Format(EmailTemplates.ProjectPostTitle, wall.Name);
                default:
                    return string.Format(EmailTemplates.DefaultPostTitle, wall.Name);
            }
        }

        private string GetPostLink(WallType wallType, int wallId, string orgName, int postId)
        {
            switch (wallType)
            {
                case WallType.Events:
                    var eventId = _eventsDbSet
                        .Where(x => x.WallId == wallId)
                        .Select(x => x.Id)
                        .First();
                    return _appSettings.EventUrl(orgName, eventId.ToString());
                case WallType.Project:
                    var projectId = _projectsDbSet
                        .Where(x => x.WallId == wallId)
                        .Select(x => x.Id)
                        .First();
                    return _appSettings.ProjectUrl(orgName, projectId.ToString());
                default:
                    return _appSettings.WallPostUrl(orgName, postId);
            }
        }
    }
}
