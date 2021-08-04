using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Resources.Emails;
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
        private readonly IMarkdownConverter _markdownConverter;
        private readonly ILogger _logger;

        private readonly IDbSet<MultiwallWall> _wallsDbSet;
        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<Project> _projectsDbSet;

        public PostNotificationService(IUnitOfWork2 uow,
            IUserService userService,
            IPostService postService,
            IMailTemplate mailTemplate,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IOrganizationService organizationService,
            IMarkdownConverter markdownConverter,
            ILogger logger)
        {
            _appSettings = appSettings;
            _userService = userService;
            _postService = postService;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;
            _logger = logger;

            _wallsDbSet = uow.GetDbSet<DataLayer.EntityModels.Models.Multiwall.Wall>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
        }

        public async Task NotifyAboutNewPostAsync(NewlyCreatedPostDTO post)
        {
            var postCreator = await _userService.GetApplicationUserAsync(post.User.UserId);

            var organization = await _organizationService.GetOrganizationByIdAsync(postCreator.OrganizationId);
            var wall = await _wallsDbSet.SingleAsync(w => w.Id == post.WallId);

            var mentionedUsers = (await GetMentionedUsersAsync(post.MentionedUsersIds)).ToList();
            var destinationEmails = (await _userService.GetWallUsersEmailsAsync(postCreator.Email, wall))
                .Except(mentionedUsers.Select(x => x.Email)).ToList();

            if (destinationEmails.Count > 0)
            {
                await SendWallSubscriberEmailsAsync(post, destinationEmails, postCreator, organization, wall);
            }

            if (mentionedUsers.Count > 0)
            {
                await SendMentionEmailsAsync(post, mentionedUsers, postCreator, organization);
            }
        }

        private async Task SendWallSubscriberEmailsAsync(NewlyCreatedPostDTO post,
            IEnumerable<string> destinationEmails,
            ApplicationUser postCreator,
            Organization organization,
            MultiwallWall wall)
        {
            var postLink = await GetPostLinkAsync(post.WallType, post.WallId, organization.ShortName, post.Id);
            var authorPictureUrl = _appSettings.PictureUrl(organization.ShortName, postCreator.PictureId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var subject = string.Format(Templates.NewWallPostEmailSubject, wall.Name, postCreator.FullName);
            var body = _markdownConverter.ConvertToHtml(post.MessageBody);

            var emailTemplateViewModel = new NewWallPostEmailTemplateViewModel(GetWallTitle(wall),
                authorPictureUrl,
                postCreator.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                GetActionButtonTitle(wall));

            var content = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.NewWallPost);

            var emailData = new EmailDto(destinationEmails, subject, content);
            await _mailingService.SendEmailAsync(emailData);
        }

        private async Task SendMentionEmailsAsync(NewlyCreatedPostDTO post, IEnumerable<ApplicationUser> mentionedUsers, ApplicationUser postCreator, Organization organization)
        {
            var messageBody = _markdownConverter.ConvertToHtml(await _postService.GetPostBodyAsync(post.Id));
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var postUrl = _appSettings.WallPostUrl(organization.ShortName, post.Id);
            const string subject = "You have been mentioned in the post";

            foreach (var mentionedUser in mentionedUsers)
            {
                try
                {
                    if (mentionedUser.NotificationsSettings?.MentionEmailNotifications == false)
                    {
                        continue;
                    }

                    var newMentionTemplateViewModel = new NewMentionTemplateViewModel(mentionedUser.FullName,
                        postCreator.FacebookEmail,
                        postUrl,
                        userNotificationSettingsUrl,
                        messageBody);

                    var content = _mailTemplate.Generate(newMentionTemplateViewModel, EmailTemplateCacheKeys.NewMention);

                    var emailData = new EmailDto(mentionedUser.Email, subject, content);
                    await _mailingService.SendEmailAsync(emailData);
                }
                catch (Exception e)
                {
                    _logger.Debug(e.Message, e);
                }
            }
        }

        private async Task<IEnumerable<ApplicationUser>> GetMentionedUsersAsync(IEnumerable<string> mentionedUsersIds)
        {
            if (mentionedUsersIds == null)
            {
                return Enumerable.Empty<ApplicationUser>();
            }

            var users = await _userService.GetUsersWithMentionNotificationsAsync(mentionedUsersIds);
            return users;
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
