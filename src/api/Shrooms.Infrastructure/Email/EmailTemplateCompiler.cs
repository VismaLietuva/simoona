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

        private readonly string _baseDir;

        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> _compiledLayout;
        // TODO: test out after clean up
        public EmailTemplateCompiler(IMailTemplateCache mailTemplateCache, string baseDir)
        {
            _mailTemplateCache = mailTemplateCache;
            _baseDir = baseDir;

            _razorEngine = new RazorEngineCore.RazorEngine();
        }

        public void Register()
        {
            _compiledLayout = CompileLayout();

            AddAndCompile<NewWallPostEmailTemplateViewModel>(@"EmailTemplates\Wall\NewPost.cshtml");
            AddAndCompile<NewMentionTemplateViewModel>(@"EmailTemplates\Wall\NewMention.cshtml");
            AddAndCompile<KudosRejectedEmailTemplateViewModel>(@"EmailTemplates\Kudos\KudosRejected.cshtml");
            AddAndCompile<KudosSentEmailTemplateViewModel>(@"EmailTemplates\Kudos\KudosSent.cshtml");
            AddAndCompile<KudosReceivedEmailTemplateViewModel>(@"EmailTemplates\Kudos\KudosReceived.cshtml");
            AddAndCompile<KudosDecreasedEmailTemplateViewModel>(@"EmailTemplates\Kudos\KudosDecreased.cshtml");
            AddAndCompile<BirthdaysNotificationTemplateViewModel>(@"EmailTemplates\BirthdaysNotification.cshtml");
            AddAndCompile<UserConfirmationEmailTemplateViewModel>(@"EmailTemplates\AdministrationUsers\UserConfirmation.cshtml");
            AddAndCompile<NotificationAboutNewUserEmailTemplateViewModel>(@"EmailTemplates\AdministrationUsers\NotificationAboutNewUser.cshtml");
            AddAndCompile<NewCommentEmailTemplateViewModel>(@"EmailTemplates\Wall\NewComment.cshtml");
            AddAndCompile<ResetPasswordTemplateViewModel>(@"EmailTemplates\AdministrationUsers\UserResetPassword.cshtml");
            AddAndCompile<VerifyEmailTemplateViewModel>(@"EmailTemplates\AdministrationUsers\UserVerifyEmail.cshtml");
            
            // Premium
            AddAndCompile<BookReminderEmailTemplateViewModel>(@"EmailTemplates\Books\BookRemind.cshtml");
            AddAndCompile<BookReportEmailTemplateViewModel>(@"EmailTemplates\Books\BookReport.cshtml");
            AddAndCompile<EventParticipantExpelledEmailTemplateViewModel>(@"EmailTemplates\Events\ParticipantExpelled.cshtml");
            AddAndCompile<EventJoinRemindEmailTemplateViewModel>(@"EmailTemplates\Events\RemindToJoin.cshtml");
            AddAndCompile<CoacheeJoinedEventEmailTemplateViewModel>(@"EmailTemplates\Events\CoacheeJoinedEvent.cshtml", builder => builder.AddUsing(nameof(System)));
            AddAndCompile<CoacheeLeftEventEmailTemplateViewModel>(@"EmailTemplates\Events\CoacheeLeftEvent.cshtml");
            AddAndCompile<BookTakenEmailTemplateViewModel>(@"EmailTemplates\Books\BookTaken.cshtml");
            AddAndCompile<LoyaltyKudosReceivedEmailTemplateViewModel>(@"EmailTemplates\LoyaltyKudos\LoyaltyKudosReceived.cshtml");
            AddAndCompile<LoyaltyKudosDecreasedEmailTemplateViewModel>(@"EmailTemplates\LoyaltyKudos\LoyaltyKudosDecreased.cshtml");
            AddAndCompile<CommitteeSuggestionEmailTemplateViewModel>(@"EmailTemplates\Committees\CommitteesSuggestion.cshtml");
            AddAndCompile<ServiceRequestCommentEmailTemplateViewModel>(@"EmailTemplates\ServiceRequests\ServiceRequestComment.cshtml");
            AddAndCompile<ServiceRequestEmailTemplateViewModel>(@"EmailTemplates\ServiceRequests\NewServiceRequest.cshtml");
            AddAndCompile<ServiceRequestUpdateEmailTemplateViewModel>(@"EmailTemplates\ServiceRequests\UpdateServiceRequest.cshtml");
            AddAndCompile<StartedLotteryEmailTemplateViewModel>(@"EmailTemplates\Lotteries\StartedLottery.cshtml");
        }

        private string ReadEmailTemplate(string relativePath)
        {
            return File.ReadAllText(Path.Combine(_baseDir, relativePath));
        }

        private IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> CompileLayout()
        {
            var headerFooterTemplate = ReadEmailTemplate(@"EmailTemplates\HeaderFooter.cshtml");

            return _razorEngine.Compile<EmailTemplateBase<LayoutEmailTemplateViewModel>>(headerFooterTemplate);
        }

        private void AddAndCompile<T>(string relativePath, Action<IRazorEngineCompilationOptionsBuilder> builder = null) where T : BaseEmailTemplateViewModel
        {
            var emailTemplate = ReadEmailTemplate(relativePath);

            var compiledTemplate = _razorEngine.Compile<T>(
                emailTemplate,
                _compiledLayout,
                builder);

            _mailTemplateCache.Add<T>(compiledTemplate);
        }
    }
}