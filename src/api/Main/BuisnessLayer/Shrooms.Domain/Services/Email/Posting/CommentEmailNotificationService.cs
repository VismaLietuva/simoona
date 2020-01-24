using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Host.Contracts.Infrastructure.Email;
using Shrooms.Resources.Emails;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class CommentEmailNotificationService : ICommentEmailNotificationService
    {
        private readonly IUserService _userService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IPostService _postService;
        private readonly IMarkdownConverter _markdownConverter;

        private readonly IDbSet<EntityModels.Models.Events.Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;
        private readonly IDbSet<Comment> _commentsDbSet;

        public CommentEmailNotificationService(IUnitOfWork2 uow,
                                          IUserService userService,
                                          IMailTemplate mailTemplate,
                                          IMailingService mailingService,
                                          IApplicationSettings appSettings,
                                          IOrganizationService organizationService,
                                          IMarkdownConverter markdownConverter,
                                          IPostService postService)
        {
            _appSettings = appSettings;
            _userService = userService;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;
            _postService = postService;

            _eventsDbSet = uow.GetDbSet<EntityModels.Models.Events.Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
            _commentsDbSet = uow.GetDbSet<Comment>();
        }

        public void SendEmailNotification(CommentCreatedDTO commentDto)
        {
            var commentCreator = _userService.GetApplicationUser(commentDto.CommentCreator);
            var organization = _organizationService.GetOrganizationById(commentCreator.OrganizationId);

            var destinationEmails = GetPostWatchersEmails(commentCreator.Email, commentDto.PostId, commentCreator.Id);

            if (destinationEmails.Count <= 0)
            {
                return;
            }

            var comment = LoadComment(commentDto.CommentId);
            var postLink = GetPostLink(commentDto.WallType, commentDto.WallId, organization.ShortName, commentDto.PostId);
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

        private IList<string> GetPostWatchersEmails(string senderEmail, int postId, string commentCreatorId)
        {
            var postWatchers = _postService.GetPostWatchersForEmailNotifications(postId).ToList();

            return postWatchers
                .Where(u => u.Email != senderEmail && u.Id != commentCreatorId)
                .Select(u => u.Email)
                .Distinct()
                .ToList();
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

                case WallType.Main:
                case WallType.UserCreated:
                default:
                    return _appSettings.WallPostUrl(orgName, postId);
            }
        }

        private Comment LoadComment(int commentId)
        {
            var comment = _commentsDbSet
                .Include(x => x.Post)
                .FirstOrDefault(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, $"Comment {commentId} does not exist");
            }

            return comment;
        }
    }
}