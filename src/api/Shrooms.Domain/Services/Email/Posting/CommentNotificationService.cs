using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Resources.Models.Walls.Comments;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Resources.Emails;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class CommentNotificationService : NotificationServiceBase, ICommentNotificationService
    {
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IPostService _postService;
        private readonly IMarkdownConverter _markdownConverter;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;
        private readonly IDbSet<Comment> _commentsDbSet;

        public CommentNotificationService(
            IUnitOfWork2 uow,
            IUserService userService,
            ICommentService commentService,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService,
            IMarkdownConverter markdownConverter,
            IPostService postService)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;
            _userService = userService;
            _commentService = commentService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;
            _postService = postService;

            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
            _commentsDbSet = uow.GetDbSet<Comment>();
        }

        public async Task NotifyAboutNewCommentAsync(CommentCreatedDto commentDto)
        {
            var commentAuthor = await _userService.GetApplicationUserAsync(commentDto.CommentAuthor);
            var organization = await _organizationService.GetOrganizationByIdAsync(commentAuthor.OrganizationId);

            var mentionedUsers = await _userService.GetUsersWithMentionNotificationsAsync(commentDto.MentionedUserIds.Distinct());

            var destinationEmails = (await GetPostWatchersEmailsAsync(commentAuthor.Email, commentDto.PostId, commentAuthor.Id))
                .Except(mentionedUsers.Select(x => x.Email))
                .ToList();

            if (destinationEmails.Any())
            {
                await SendPostWatcherEmailsAsync(commentDto, destinationEmails, commentAuthor, organization);
            }

            if (mentionedUsers.Any())
            {
                await SendMentionedUserEmailsAsync(
                    commentDto.CommentId,
                    commentDto.PostId,
                    commentAuthor.FullName,
                    mentionedUsers,
                    organization.ShortName);
            }
        }

        public async Task NotifyMentionedUsersAsync(EditCommentDto editCommentDto)
        {
            var mentionCommentDto = await _commentService.GetMentionCommentByIdAsync(editCommentDto.Id);
            var organization = await _organizationService.GetOrganizationByIdAsync(editCommentDto.OrganizationId);
            var mentionedUsers = await _userService.GetUsersWithMentionNotificationsAsync(editCommentDto.MentionedUserIds.Distinct());

            await SendMentionedUserEmailsAsync(
                editCommentDto.Id,
                mentionCommentDto.PostId,
                mentionCommentDto.AuthorFullName,
                mentionedUsers,
                organization.ShortName);
        }

        private async Task SendMentionedUserEmailsAsync(int commentId, int postId, string commentAuthorFullName, IEnumerable<ApplicationUser> mentionedUsers, string organizationShortName)
        {
            var comment = await _commentService.GetCommentBodyAsync(commentId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationShortName);
            var postUrl = _appSettings.WallPostUrl(organizationShortName, postId);

            var messageBody = _markdownConverter.ConvertToHtml(comment);

            foreach (var mentionedUser in mentionedUsers)
            {
                var subject = Comments.NewMentionEmailSubject;
                var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                    subject,
                    mentionedUser.FullName,
                    commentAuthorFullName,
                    postUrl,
                    userNotificationSettingsUrl,
                    messageBody);

                await SendSingleEmailAsync(
                    mentionedUser.Email,
                    subject,
                    newMentionTemplateViewModel,
                    EmailTemplateCacheKeys.NewMention);
            }
        }

        private async Task SendPostWatcherEmailsAsync(CommentCreatedDto commentDto, IList<string> emails, ApplicationUser commentAuthor, Organization organization)
        {
            var comment = await LoadCommentAsync(commentDto.CommentId);
            var postLink = await GetPostLinkAsync(commentDto.WallType, commentDto.WallId, organization.ShortName, commentDto.PostId);

            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, commentAuthor.PictureId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);

            var subject = CreateSubject(Templates.NewPostCommentEmailSubject, CutMessage(comment.Post.MessageBody), commentAuthor.FullName);
            var body = _markdownConverter.ConvertToHtml(comment.MessageBody);

            var emailTemplateViewModel = new NewCommentEmailTemplateViewModel(string.Format(EmailTemplates.PostCommentTitle, CutMessage(comment.Post.MessageBody)),
                authorPictureUrl,
                commentAuthor.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                EmailTemplates.DefaultActionButtonTitle);

            await SendMultipleEmailsAsync(emails, subject, emailTemplateViewModel, EmailTemplateCacheKeys.NewPostComment);
        }

        private async Task<IList<string>> GetPostWatchersEmailsAsync(string senderEmail, int postId, string commentAuthorId)
        {
            var postWatchers = await _postService.GetPostWatchersForEmailNotificationsAsync(postId);

            return postWatchers
                .Where(u => u.Email != senderEmail && u.Id != commentAuthorId)
                .Select(u => u.Email)
                .Distinct()
                .ToList();
        }

        private static string CutMessage(string value)
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

        private async Task<string> GetPostLinkAsync(WallType wallType, int wallId, string orgName, int postId)
        {
            switch (wallType)
            {
                case WallType.Events:
                    var eventId = await _eventsDbSet
                        .Where(x => x.WallId == wallId)
                        .Select(x => x.Id)
                        .FirstAsync();

                    return _appSettings.EventUrl(orgName, eventId.ToString());

                case WallType.Project:
                    var projectId = await _projectsDbSet
                        .Where(x => x.WallId == wallId)
                        .Select(x => x.Id)
                        .FirstAsync();

                    return _appSettings.ProjectUrl(orgName, projectId.ToString());

                case WallType.Main:
                case WallType.UserCreated:
                default:
                    return _appSettings.WallPostUrl(orgName, postId);
            }
        }

        private async Task<Comment> LoadCommentAsync(int commentId)
        {
            var comment = await _commentsDbSet
                .Include(x => x.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, $"Comment {commentId} does not exist");
            }

            return comment;
        }
    }
}
