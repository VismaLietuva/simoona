﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Resources.Emails;
using Shrooms.Resources.Models.Walls.Posts;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class PostNotificationService : IPostNotificationService
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;

        private readonly IDbSet<MultiwallWall> _wallsDbSet;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;

        public PostNotificationService(
            IUnitOfWork2 uow,
            IUserService userService,
            IPostService postService,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService)
        {
            _appSettings = appSettings;
            _userService = userService;
            _postService = postService;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;

            _wallsDbSet = uow.GetDbSet<MultiwallWall>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
        }

        public async Task NotifyAboutNewPostAsync(NewlyCreatedPostDto post)
        {
            var postAuthor = await _userService.GetApplicationUserAsync(post.User.UserId);

            var organization = await _organizationService.GetOrganizationByIdAsync(postAuthor.OrganizationId);
            var wall = await _wallsDbSet.SingleAsync(w => w.Id == post.WallId);

            var mentionedUsers = await _userService.GetUsersWithMentionNotificationsAsync(post.MentionedUsersIds.Distinct());

            var wallUsersEmails = await _userService.GetWallUsersEmailsAsync(postAuthor.Email, wall);

            var destinationEmails = wallUsersEmails.Except(mentionedUsers.Select(x => x.Email));

            if (destinationEmails.Any())
            {
                await SendWallSubscriberEmailsAsync(post, destinationEmails, postAuthor, organization, wall);
            }

            if (mentionedUsers.Any())
            {
                await SendMentionerUserEmailsAsync(post.Id, postAuthor.FullName, mentionedUsers, organization.ShortName);
            }
        }

        public async Task NotifyMentionedUsersAsync(EditPostDto editPostDto)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(editPostDto.OrganizationId);
            var postAuthor = await _postService.GetPostCreatorByIdAsync(editPostDto.Id);
            var mentionedUsers = await _userService.GetUsersWithMentionNotificationsAsync(editPostDto.MentionedUserIds.Distinct());
        
            await SendMentionerUserEmailsAsync(editPostDto.Id, postAuthor.FullName, mentionedUsers, organization.ShortName);
        }

        private async Task SendMentionerUserEmailsAsync(int postId, string postAuthorFullName, IEnumerable<ApplicationUser> mentionedUsers, string organizationShortName)
        {
            var postBody = await _postService.GetPostBodyAsync(postId);

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationShortName);
            var postUrl = _appSettings.WallPostUrl(organizationShortName, postId);

            foreach (var mentionedUser in mentionedUsers)
            {
                var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                    Posts.NewMentionEmailSubject,
                    mentionedUser.FullName,
                    postAuthorFullName,
                    postUrl,
                    userNotificationSettingsUrl,
                    postBody);

                var content = _mailTemplate.Generate(newMentionTemplateViewModel);

                await _mailingService.SendEmailAsync(new EmailDto(mentionedUser.Email, Posts.NewMentionEmailSubject, content));
            }
        }

        private async Task SendWallSubscriberEmailsAsync(
            NewlyCreatedPostDto post,
            IEnumerable<string> destinationEmails,
            ApplicationUser postCreator,
            Organization organization,
            MultiwallWall wall)
        {
            var postLink = await GetPostLinkAsync(post.WallType, post.WallId, organization.ShortName, post.Id);
            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, postCreator.PictureId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var subject = string.Format(Templates.NewWallPostEmailSubject, wall.Name, postCreator.FullName);

            var emailTemplateViewModel = new NewWallPostEmailTemplateViewModel(GetWallTitle(wall),
                authorPictureUrl,
                postCreator.FullName,
                postLink,
                post.MessageBody,
                userNotificationSettingsUrl,
                GetActionButtonTitle(wall));

            var content = _mailTemplate.Generate(emailTemplateViewModel);

            var emailData = new EmailDto(destinationEmails, subject, content);
            await _mailingService.SendEmailAsync(emailData);
        }

        private static string GetActionButtonTitle(MultiwallWall wall)
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

        private static string GetWallTitle(MultiwallWall wall)
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

                default:
                    return _appSettings.WallPostUrl(orgName, postId);
            }
        }
    }
}
