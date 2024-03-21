using System;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email.Templating
{
    public class EmailTemplatesCompiler : IEmailTemplateCompiler
    {
        private static string _baseDir;

        public void Register(string baseDir)
        {
            _baseDir = Path.Combine(baseDir, "bin");

            Engine.Razor.AddTemplate(EmailTemplateCacheKeys.HeaderFooterLayout, File.ReadAllText(Path.Combine(_baseDir, @"EmailTemplates\HeaderFooter.cshtml")));

            AddAndCompile(EmailTemplateCacheKeys.NewWallPost, @"EmailTemplates\Wall\NewPost.cshtml", typeof(NewWallPostEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NewMention, @"EmailTemplates\Wall\NewMention.cshtml", typeof(NewMentionTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosRejected, @"EmailTemplates\Kudos\KudosRejected.cshtml", typeof(KudosRejectedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosSent, @"EmailTemplates\Kudos\KudosSent.cshtml", typeof(KudosSentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosReceived, @"EmailTemplates\Kudos\KudosReceived.cshtml", typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosDecreased, @"EmailTemplates\Kudos\KudosDecreased.cshtml", typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.BirthdaysNotification, @"EmailTemplates\BirthdaysNotification.cshtml", typeof(BirthdaysNotificationTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.UserConfirmation, @"EmailTemplates\AdministrationUsers\UserConfirmation.cshtml", typeof(UserConfirmationEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NotificationAboutNewUser, @"EmailTemplates\AdministrationUsers\NotificationAboutNewUser.cshtml", typeof(NotificationAboutNewUserEmailTemplateViewModel));
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
