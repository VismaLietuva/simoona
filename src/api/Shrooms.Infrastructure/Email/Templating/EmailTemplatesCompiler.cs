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
            _baseDir = baseDir;

            Engine.Razor.AddTemplate(EmailTemplateCacheKeys.HeaderFooterLayout, File.ReadAllText(Path.Combine(_baseDir, "EmailTemplates", "HeaderFooter.html")));

            AddAndCompile(EmailTemplateCacheKeys.NewWallPost, Path.Combine("EmailTemplates", "Wall", "NewPost.html"), typeof(NewWallPostEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NewMention, Path.Combine("EmailTemplates", "Wall", "NewMention.html"), typeof(NewMentionTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosRejected, Path.Combine("EmailTemplates", "Kudos", "KudosRejected.html"), typeof(KudosRejectedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosSent, Path.Combine("EmailTemplates", "Kudos", "KudosSent.html"), typeof(KudosSentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosReceived, Path.Combine("EmailTemplates", "Kudos", "KudosReceived.html"), typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.KudosDecreased, Path.Combine("EmailTemplates", "Kudos", "KudosDecreased.html"), typeof(KudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.BirthdaysNotification, Path.Combine("EmailTemplates", "BirthdaysNotification.html"), typeof(BirthdaysNotificationTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.UserConfirmation, Path.Combine("EmailTemplates", "AdministrationUsers", "UserConfirmation.html"), typeof(UserConfirmationEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NotificationAboutNewUser, Path.Combine("EmailTemplates", "AdministrationUsers", "NotificationAboutNewUser.html"), typeof(NotificationAboutNewUserEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.NewPostComment, Path.Combine("EmailTemplates", "Wall", "NewComment.html"), typeof(NewCommentEmailTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.ResetPassword, Path.Combine("EmailTemplates", "AdministrationUsers", "UserResetPassword.html"), typeof(ResetPasswordTemplateViewModel));
            AddAndCompile(EmailTemplateCacheKeys.VerifyEmail, Path.Combine("EmailTemplates", "AdministrationUsers", "UserVerifyEmail.html"), typeof(VerifyEmailTemplateViewModel));
        }

        private static void AddAndCompile(string templateKey, string relativePath, Type templateViewModel)
        {
            var absolutePath = Path.Combine(_baseDir, relativePath);

            Engine.Razor.AddTemplate(templateKey, File.ReadAllText(absolutePath));
            Engine.Razor.Compile(templateKey, templateViewModel);
        }
    }
}
