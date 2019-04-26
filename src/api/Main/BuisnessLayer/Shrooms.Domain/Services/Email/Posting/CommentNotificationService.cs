using System;
using System.Data.Entity;
using System.Linq;
using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
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
    public class CommentNotificationService : ICommentNotificationService
    {
        private readonly IUserService _userService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IMarkdownConverter _markdownConverter;

        private readonly IDbSet<EntityModels.Models.Events.Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;

        public CommentNotificationService(
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

            _eventsDbSet = uow.GetDbSet<EntityModels.Models.Events.Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
        }

        public void NotifyAboutNewComment(Comment comment, ApplicationUser commentCreator)
        {
            var organization = _organizationService.GetOrganizationById(commentCreator.OrganizationId);

            var destinationEmails = _userService.GetPostCommentersEmails(commentCreator.Email, comment.PostId);
            var postAuthorEmail = (comment.Post.AuthorId == comment.AuthorId) ? null : _userService.GetPostAuthorEmail(comment.Post.AuthorId);
            if (postAuthorEmail != null && destinationEmails.Contains(postAuthorEmail) == false)
            {
                destinationEmails.Add(postAuthorEmail);
            }

            if (destinationEmails.Count > 0)
            {
                var postLink = GetPostLink(comment.Post.Wall, organization.ShortName, comment.Post);
                var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, commentCreator.PictureId);
                var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
                var subject = string.Format(Templates.NewPostCommentEmailSubject, CutMessage(comment.Post.MessageBody), commentCreator.FullName);
                var body = _markdownConverter.ConvertToHtml(comment.MessageBody);

                var emailTemplateViewModel = new NewCommentEmailTemplateViewModel(
                    string.Format(Constants.BusinessLayer.Templates.PostCommentTitle, CutMessage(comment.Post.MessageBody)),
                    authorPictureUrl,
                    commentCreator.FullName,
                    postLink,
                    body,
                    userNotificationSettingsUrl,
                    Constants.BusinessLayer.Templates.DefautlActionButtonTitle);

                var content = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.NewPostComment);
                var emailData = new EmailDto(destinationEmails, subject, content);
                _mailingService.SendEmail(emailData);
            }
        }

        private string CutMessage(string value)
        {
            var newLine = value.IndexOf("\n", StringComparison.Ordinal);
            if (newLine > 0 && newLine <= 30)
            {
                return value.Substring(0, newLine) + "...";
            }

            if (value.Length > 30)
            {
                return value.Substring(0, 30) + "...";
            }

            return value;
        }

        private string GetPostLink(EntityModels.Models.Multiwall.Wall wall, string orgName, Post post)
        {
            switch (wall.Type)
            {
                case WallType.Events:
                    var eventId = _eventsDbSet
                        .Where(x => x.WallId == wall.Id)
                        .Select(x => x.Id)
                        .First();
                    return _appSettings.EventUrl(orgName, eventId.ToString());
                case WallType.Project:
                    var projectId = _projectsDbSet
                        .Where(x => x.WallId == wall.Id)
                        .Select(x => x.Id)
                        .First();
                    return _appSettings.ProjectUrl(orgName, projectId.ToString());
                default:
                    return _appSettings.WallPostUrl(orgName, post.Id);
            }
        }
    }
}
