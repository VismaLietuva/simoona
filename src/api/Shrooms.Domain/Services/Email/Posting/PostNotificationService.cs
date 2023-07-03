using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
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
using Shrooms.Resources.Models.Walls.Posts;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public class PostNotificationService : NotificationServiceBase, IPostNotificationService
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IApplicationSettings _appSettings;
        private readonly IOrganizationService _organizationService;
        private readonly IMarkdownConverter _markdownConverter;

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
            IOrganizationService organizationService,
            IMarkdownConverter markdownConverter)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;
            _userService = userService;
            _postService = postService;
            _organizationService = organizationService;
            _markdownConverter = markdownConverter;

            _wallsDbSet = uow.GetDbSet<MultiwallWall>();
            _eventsDbSet = uow.GetDbSet<Event>();
            _projectsDbSet = uow.GetDbSet<Project>();
        }

        public async Task NotifyAboutNewPostAsync(NewlyCreatedPostDto post)
        {
            var postAuthor = await _userService.GetApplicationUserAsync(post.User.UserId);

            var organization = await _organizationService.GetOrganizationByIdAsync(postAuthor.OrganizationId);
            var wall = await _wallsDbSet.SingleAsync(w => w.Id == post.WallId);

            var mentionedUsers = (await _userService.GetUsersWithMentionNotificationsAsync(post.MentionedUsersIds.Distinct())).ToList();

            var wallUsersEmails = await _userService.GetWallUsersEmailsAsync(postAuthor.Email, wall);

            var destinationEmails = wallUsersEmails.Except(mentionedUsers.Select(x => x.Email)).ToList();

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
            var convertedPostBody = _markdownConverter.ConvertToHtml(postBody);

            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationShortName);
            var postUrl = _appSettings.WallPostUrl(organizationShortName, postId);
            var subject = Posts.NewMentionEmailSubject;

            foreach (var mentionedUser in mentionedUsers)
            {
                var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                    subject,
                    mentionedUser.FullName,
                    postAuthorFullName,
                    postUrl,
                    userNotificationSettingsUrl,
                    convertedPostBody);

                await SendSingleEmailAsync(
                    mentionedUser.Email,
                    subject,
                    newMentionTemplateViewModel,
                    EmailTemplateCacheKeys.NewMention);
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
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var subject = CreateSubject(Templates.NewWallPostEmailSubject, wall.Name, postCreator.FullName);
            var body = _markdownConverter.ConvertToHtml(post.MessageBody);

            var emailTemplateViewModel = new NewWallPostEmailTemplateViewModel(GetWallTitle(wall),
                authorPictureUrl,
                postCreator.FullName,
                postLink,
                body,
                userNotificationSettingsUrl,
                GetActionButtonTitle(wall));

            await SendMultipleEmailsAsync(
                destinationEmails,
                subject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.NewWallPost);
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
