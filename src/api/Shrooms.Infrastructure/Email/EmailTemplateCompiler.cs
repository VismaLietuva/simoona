using System.IO;
using RazorEngineCore;
using Shrooms.Infrastructure.Extensions;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Contracts.DataTransferObjects;
using System;
using Shrooms.Infrastructure.Email.Templates;

namespace Shrooms.Infrastructure.Email
{
    public class EmailTemplateCompiler : IEmailTemplateCompiler
    {
        private readonly IMailTemplateCache _mailTemplateCache;
        private readonly IRazorEngine _razorEngine;
        private readonly IEmailTemplateConfiguration _emailTemplateConfiguration;

        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> _compiledLayout;
        
        public EmailTemplateCompiler(IMailTemplateCache mailTemplateCache, IEmailTemplateConfiguration emailTemplateConfiguration)
        {
            _mailTemplateCache = mailTemplateCache;
            _emailTemplateConfiguration = emailTemplateConfiguration;

            _razorEngine = new RazorEngine();
        }

        public void Register()
        {
            _compiledLayout = CompileLayout();

            AddAndCompile<NewWallPostEmailTemplateViewModel>(_emailTemplateConfiguration.NewWallPostEmailTemplateAbsolutePath);
            AddAndCompile<NewMentionTemplateViewModel>(_emailTemplateConfiguration.NewMentionTemplateAbsolutePath);
            AddAndCompile<KudosRejectedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosRejectedEmailTemplateAbsolutePath);
            AddAndCompile<KudosSentEmailTemplateViewModel>(_emailTemplateConfiguration.KudosSentEmailTemplateAbsolutePath);
            AddAndCompile<KudosReceivedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosReceivedEmailTemplateAbsolutePath);
            AddAndCompile<KudosDecreasedEmailTemplateViewModel>(_emailTemplateConfiguration.KudosDecreasedEmailTemplateAbsolutePath);
            AddAndCompile<BirthdaysNotificationTemplateViewModel>(_emailTemplateConfiguration.BirthdaysNotificationAbsolutePath);
            AddAndCompile<UserConfirmationEmailTemplateViewModel>(_emailTemplateConfiguration.UserConfirmationEmailTemplateAbsolutePath);
            AddAndCompile<NotificationAboutNewUserEmailTemplateViewModel>(_emailTemplateConfiguration.NotificationAboutNewUserEmailTemplateAbsolutePath);
            AddAndCompile<NewCommentEmailTemplateViewModel>(_emailTemplateConfiguration.NewCommentEmailTemplateAbsolutePath);
            AddAndCompile<ResetPasswordTemplateViewModel>(_emailTemplateConfiguration.ResetPasswordTemplateAbsolutePath);
            AddAndCompile<VerifyEmailTemplateViewModel>(_emailTemplateConfiguration.VerifyEmailTemplateAbsolutePath);

            // Premium
            AddAndCompile<BookReminderEmailTemplateViewModel>(_emailTemplateConfiguration.BookReminderEmailTemplateAbsolutePath);
            AddAndCompile<BookReportEmailTemplateViewModel>(_emailTemplateConfiguration.BookReportEmailTemplateAbsolutePath);
            AddAndCompile<EventParticipantExpelledEmailTemplateViewModel>(_emailTemplateConfiguration.EventParticipantExpelledEmailTemplateAbsolutePath);
            AddAndCompile<EventJoinRemindEmailTemplateViewModel>(_emailTemplateConfiguration.EventJoinRemindEmailTemplateAbsolutePath);
            AddAndCompile<CoacheeJoinedEventEmailTemplateViewModel>(_emailTemplateConfiguration.CoacheeJoinedEventEmailTemplateAbsolutePath, builder => builder.AddUsing(nameof(System)));
            AddAndCompile<CoacheeLeftEventEmailTemplateViewModel>(_emailTemplateConfiguration.CoacheeLeftEventEmailTemplateAbsolutePath);
            AddAndCompile<BookTakenEmailTemplateViewModel>(_emailTemplateConfiguration.BookTakenEmailTemplateAbsolutePath);
            AddAndCompile<LoyaltyKudosReceivedEmailTemplateViewModel>(_emailTemplateConfiguration.LoyaltyKudosReceivedEmailTemplateAbsolutePath);
            AddAndCompile<LoyaltyKudosDecreasedEmailTemplateViewModel>(_emailTemplateConfiguration.LoyaltyKudosDecreasedEmailTemplateAbsolutePath);
            AddAndCompile<CommitteeSuggestionEmailTemplateViewModel>(_emailTemplateConfiguration.CommitteeSuggestionEmailTemplateAbsolutePath);
            AddAndCompile<ServiceRequestCommentEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestCommentEmailTemplateAbsolutePath);
            AddAndCompile<ServiceRequestEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestEmailTemplateAbsolutePath);
            AddAndCompile<ServiceRequestUpdateEmailTemplateViewModel>(_emailTemplateConfiguration.ServiceRequestUpdateEmailTemplateAbsolutePath);
            AddAndCompile<StartedLotteryEmailTemplateViewModel>(_emailTemplateConfiguration.StartedLotteryEmailTemplateAbsolutePath);
        }

        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> CompileLayout()
        {
            return _razorEngine.Compile<EmailTemplateBase<LayoutEmailTemplateViewModel>>(File.ReadAllText(_emailTemplateConfiguration.LayoutEmailTemplateAbsolutePath));
        }

        private void AddAndCompile<T>(string absolutePath, Action<IRazorEngineCompilationOptionsBuilder> builder = null) where T : BaseEmailTemplateViewModel
        {
            var emailTemplate = File.ReadAllText(absolutePath);

            var compiledTemplate = _razorEngine.Compile<T>(
                emailTemplate,
                _compiledLayout,
                builder);

            _mailTemplateCache.Add<T>(compiledTemplate);
        }
    }
}