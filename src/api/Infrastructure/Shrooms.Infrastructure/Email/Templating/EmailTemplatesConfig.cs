using System;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.Infrastructure.Email.Templating
{
    public static class EmailTemplatesConfig
    {
        private static string _baseDir;

        public static void Register(string baseDir)
        {
            _baseDir = baseDir;

            Engine.Razor.AddTemplate(EmailTemplateCacheKeys.HeaderFooterLayout, File.ReadAllText(Path.Combine(_baseDir, @"EmailTemplates\HeaderFooter.cshtml")));

            AddAndCompile(EmailTemplateCacheKeys.NewWallPost, @"EmailTemplates\Wall\NewPost.cshtml", typeof(NewWallPostEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.EventParticipantExpelled, @"EmailTemplates\Events\ParticipantExpelled.cshtml", typeof(EventParticipantExpelledEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.BookTaken, @"EmailTemplates\Books\BookTaken.cshtml", typeof(BookTakenEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosRejected, @"EmailTemplates\Kudos\KudosRejected.cshtml", typeof(KudosRejectedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosSent, @"EmailTemplates\Kudos\KudosSent.cshtml", typeof(KudosSentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosReceived, @"EmailTemplates\Kudos\KudosReceived.cshtml", typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosDecreased, @"EmailTemplates\Kudos\KudosDecreased.cshtml", typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.BirthdaysNotification, @"EmailTemplates\BirthdaysNotification.cshtml", typeof(BirthdaysNotificationTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.CommitteeSuggestion, @"EmailTemplates\Committees\CommitteesSuggestion.cshtml", typeof(CommitteeSuggestionEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.UserConfirmation, @"EmailTemplates\AdministrationUsers\UserConfirmation.cshtml", typeof(UserConfirmationEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NotificationAboutNewUser, @"EmailTemplates\AdministrationUsers\NotificationAboutNewUser.cshtml", typeof(NotificationAboutNewUserEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.ServiceRequestComment, @"EmailTemplates\ServiceRequests\ServiceRequestComment.cshtml", typeof(ServiceRequestCommentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.ServiceRequest, @"EmailTemplates\ServiceRequests\NewServiceRequest.cshtml", typeof(ServiceRequestEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.ServiceRequestUpdate, @"EmailTemplates\ServiceRequests\UpdateServiceRequest.cshtml", typeof(ServiceRequestUpdateEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NewPostComment, @"EmailTemplates\Wall\NewComment.cshtml", typeof(NewCommentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.ResetPassword, @"EmailTemplates\AdministrationUsers\UserResetPassword.cshtml", typeof(ResetPasswordTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.VerifyEmail, @"EmailTemplates\AdministrationUsers\UserVerifyEmail.cshtml", typeof(VerifyEmailTemplateViewModel));
        }

        private static void AddAndCompile(string templateKey, string relativePath, Type templateViewModel)
        {
            var absolutePath = Path.Combine(_baseDir, relativePath);

            Engine.Razor.AddTemplate(templateKey, File.ReadAllText(absolutePath));
            Engine.Razor.Compile(templateKey, templateViewModel);
        }
    }
}