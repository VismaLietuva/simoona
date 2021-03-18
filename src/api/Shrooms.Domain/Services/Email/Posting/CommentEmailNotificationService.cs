﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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
using Shrooms.Domain.Services.Wall.Posts.Comments;
using Shrooms.Resources.Emails;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class CommentEmailNotificationService : ICommentEmailNotificationService
    {
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IPostService _postService;
        private readonly IMarkdownConverter _markdownConverter;
        private readonly ILogger _logger;

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;
        private readonly IDbSet<Comment> _commentsDbSet;

        public CommentEmailNotificationService(IUnitOfWork2 uow,
                                          IUserService userService,
                                          ICommentService commentService,
                                          IMailTemplate mailTemplate,
                                          IMailingService mailingService,
                                          IApplicationSettings appSettings,
                                          IOrganizationService organizationService,
                                          IMarkdownConverter markdownConverter,
                                          IPostService postService,
                                          ILogger logger)
        {
            _appSettings = appSettings;
            _userService = userService;
            _commentService = commentService;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;
            _postService = postService;
            _logger = logger;

            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
            _commentsDbSet = uow.GetDbSet<Comment>();
        }

        public void SendEmailNotification(CommentCreatedDTO commentDto)
        {
            var commentCreator = _userService.GetApplicationUser(commentDto.CommentCreator);
            var organization = _organizationService.GetOrganizationById(commentCreator.OrganizationId);

            var mentionedUsers = GetMentionedUsers(commentDto.MentionedUsersIds).ToList();
            var destinationEmails = GetPostWatchersEmails(commentCreator.Email, commentDto.PostId, commentCreator.Id).Except(mentionedUsers.Select(x => x.Email)).ToList();

            if (destinationEmails.Count > 0)
            {
                SendPostWatcherEmails(commentDto, destinationEmails, commentCreator, organization);
            }

            if (mentionedUsers.Count > 0)
            {
                SendMentionEmails(commentDto, mentionedUsers, commentCreator, organization);
            }
        }

        private void SendMentionEmails(CommentCreatedDTO commentDto, IList<ApplicationUser> mentionedUsers, ApplicationUser commentCreator, Organization organization)
        {
            var comment = _commentService.GetCommentBody(commentDto.CommentId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var postUrl = _appSettings.WallPostUrl(organization.ShortName, commentDto.PostId);
            var subject = $"You have been mentioned in the post";
            var messageBody = _markdownConverter.ConvertToHtml(comment);

            foreach (var mentionedUser in mentionedUsers)
            {
                try
                {
                    if (mentionedUser.NotificationsSettings != null && !mentionedUser.NotificationsSettings.MentionEmailNotifications)
                    {
                        continue;
                    }

                    var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                        mentionedUser.FullName,
                        commentCreator.FullName,
                        postUrl,
                        userNotificationSettingsUrl,
                        messageBody);

                    var content = _mailTemplate.Generate(newMentionTemplateViewModel, EmailTemplateCacheKeys.NewMention);

                    var emailData = new EmailDto(mentionedUser.Email, subject, content);
                    _mailingService.SendEmail(emailData);
                }
                catch (Exception e)
                {
                    _logger.Debug(e.Message, e);
                }
            }
        }

        private void SendPostWatcherEmails(CommentCreatedDTO commentDto, IList<string> emails, ApplicationUser commentCreator, Organization organization)
        {
            var comment = LoadComment(commentDto.CommentId);
            var postLink = GetPostLink(commentDto.WallType, commentDto.WallId, organization.ShortName, commentDto.PostId);
            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, commentCreator.PictureId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var subject = string.Format(Templates.NewPostCommentEmailSubject, CutMessage(comment.Post.MessageBody), commentCreator.FullName);
            var body = _markdownConverter.ConvertToHtml(comment.MessageBody);

            var emailTemplateViewModel = new NewCommentEmailTemplateViewModel(
                string.Format(EmailTemplates.PostCommentTitle, CutMessage(comment.Post.MessageBody)),
                authorPictureUrl,
                commentCreator.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                EmailTemplates.DefaultActionButtonTitle);

            var content = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.NewPostComment);
            var emailData = new EmailDto(emails, subject, content);
            _mailingService.SendEmail(emailData);
        }

        public IEnumerable<ApplicationUser> GetMentionedUsers(IEnumerable<string> mentionIds)
        {
            foreach (var mentionId in mentionIds)
            {
                var user = _userService.GetApplicationUser(mentionId);

                if (user.NotificationsSettings == null || user.NotificationsSettings.MentionEmailNotifications)
                {
                    yield return user;
                }
            }
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