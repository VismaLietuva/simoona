using System.Data.Entity;
using System.Linq;
using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
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

        private readonly IDbSet<EntityModels.Models.Multiwall.Wall> _wallsDbSet;
        private readonly IDbSet<EntityModels.Models.Events.Event> _eventsDbSet;
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

            _wallsDbSet = uow.GetDbSet<EntityModels.Models.Multiwall.Wall>();
            _eventsDbSet = uow.GetDbSet<EntityModels.Models.Events.Event>();
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

        private static string GetActionButtonTitle(EntityModels.Models.Multiwall.Wall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return Constants.BusinessLayer.Templates.EventActionButtonTitle;
                case WallType.Project:
                    return Constants.BusinessLayer.Templates.ProjectActionButtonTitle;
                default:
                    return Constants.BusinessLayer.Templates.DefautlActionButtonTitle;
            }
        }

        private static string GetWallTitle(EntityModels.Models.Multiwall.Wall wall)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    return string.Format(Constants.BusinessLayer.Templates.EventPostTitle, wall.Name);
                case WallType.Project:
                    return string.Format(Constants.BusinessLayer.Templates.ProjectPostTitle, wall.Name);
                default:
                    return string.Format(Constants.BusinessLayer.Templates.DefaultPostTitle, wall.Name);
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
