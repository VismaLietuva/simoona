using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Domain.Services.UserService;
using Shrooms.Domain.Services.Wall.Posts;
using Shrooms.Domain.Services.Wall.Posts.Comments;
using System;
using System.Collections.Generic;

namespace Shrooms.Presentation.Api.BackgroundWorkers
{
    public class NewMentionNotifier : IBackgroundWorker
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IApplicationSettings _appSettings;
        private readonly IPostService _postService;
        private readonly IMailingService _mailingService;
        private readonly IMailTemplate _mailTemplate;
        private readonly ILogger _logger;
        private readonly IMarkdownConverter _markdownConverter;



        public NewMentionNotifier(IUserService userService, IMarkdownConverter markdownConverter, IMailTemplate mailTemplate, ILogger logger, IOrganizationService organizationService, IPostService postService, IMailingService mailingService, IApplicationSettings applicationSettings)
        {
            _userService = userService;
            _postService = postService;
            _mailTemplate = mailTemplate;
            _logger = logger;
            _organizationService = organizationService;
            _mailingService = mailingService;
            _appSettings = applicationSettings;
            _markdownConverter = markdownConverter;
        }

        public void NotifyNewMentionInComment(int postId, IEnumerable<MentionedUserDto> usersToNotify)
        {
            foreach (var userToNotify in usersToNotify)
            {
                try
                {
                    var mentionedUser = _userService.GetApplicationUser(userToNotify.FirstName, userToNotify.LastName);

                    if (!mentionedUser.NotificationsSettings.MentionEmailNotifications)
                    {
                        var comment = _postService.GetPostLatestComment(postId);

                        var organization = _organizationService.GetOrganizationById(mentionedUser.OrganizationId);

                        var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
                        var postUrl = _appSettings.WallPostUrl(organization.ShortName, postId);

                        var subject = $"You have been mentioned in the post";
                        var messageBody = _markdownConverter.ConvertToHtml(comment.MessageBody);

                        var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                            mentionedUser.FullName,
                            comment.Author.FullName,
                            postUrl,
                            userNotificationSettingsUrl,
                            messageBody);

                        var content = _mailTemplate.Generate(newMentionTemplateViewModel, EmailTemplateCacheKeys.NewMention);

                        var emailData = new EmailDto(mentionedUser.Email, subject, content);
                        _mailingService.SendEmail(emailData);
                    }
                }
                catch (Exception e)
                {
                    _logger.Debug(e.Message, e);
                }
            }
        }


        public void NotifyNewMentionInPost(int postId, string mentioningUser, IEnumerable<MentionedUserDto> usersToNotify)
        {
            foreach (var userToNotify in usersToNotify)
            {
                try
                {
                    var mentionedUser = _userService.GetApplicationUser(userToNotify.FirstName, userToNotify.LastName);

                    if (mentionedUser.NotificationsSettings.MentionEmailNotifications)
                    {
                        var messageBody = _markdownConverter.ConvertToHtml(_postService.GetPostBody(postId));
                        var organization = _organizationService.GetOrganizationById(mentionedUser.OrganizationId);

                        var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
                        var postUrl = _appSettings.WallPostUrl(organization.ShortName, postId);
                        var subject = $"You have been mentioned in the post";

                        var newMentionTemplateViewModel = new NewMentionTemplateViewModel(
                            mentionedUser.FullName,
                            mentioningUser,
                            postUrl,
                            userNotificationSettingsUrl,
                            messageBody);

                        var content = _mailTemplate.Generate(newMentionTemplateViewModel, EmailTemplateCacheKeys.NewMention);

                        var emailData = new EmailDto(mentionedUser.Email, subject, content);
                        _mailingService.SendEmail(emailData);
                    }
                }
                catch (Exception e)
                {
                    _logger.Debug(e.Message, e);
                }
            }
        }
    }
}