using Shrooms.Contracts.Infrastructure.Email;
using System;
using System.IO;

namespace Shrooms.Infrastructure.Email
{
    public class EmailTemplateConfiguration : IEmailTemplateConfiguration
    {
        public string LayoutEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\HeaderFooter.cshtml");

        public string NewWallPostEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Wall\NewPost.cshtml");

        public string KudosRejectedEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Kudos\KudosRejected.cshtml");

        public string KudosReceivedEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Kudos\KudosReceived.cshtml");

        public string KudosDecreasedEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Kudos\KudosDecreased.cshtml");

        public string BirthdaysNotificationAbsolutePath => GetAbsolutePath(@"EmailTemplates\BirthdaysNotification.cshtml");

        public string UserConfirmationEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\AdministrationUsers\UserConfirmation.cshtml");

        public string NotificationAboutNewUserEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\AdministrationUsers\NotificationAboutNewUser.cshtml");

        public string NewCommentEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Wall\NewComment.cshtml");

        public string ResetPasswordTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\AdministrationUsers\UserResetPassword.cshtml");

        public string VerifyEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\AdministrationUsers\UserVerifyEmail.cshtml");

        public string BookReminderEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Books\BookRemind.cshtml");

        public string BookReportEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Books\BookReport.cshtml");

        public string EventParticipantExpelledEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Events\ParticipantExpelled.cshtml");

        public string EventJoinRemindEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Events\RemindToJoin.cshtml");

        public string CoacheeJoinedEventEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Events\CoacheeJoinedEvent.cshtml");

        public string CoacheeLeftEventEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Events\CoacheeLeftEvent.cshtml");

        public string BookTakenEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Books\BookTaken.cshtml");

        public string LoyaltyKudosReceivedEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\LoyaltyKudos\LoyaltyKudosReceived.cshtml");

        public string LoyaltyKudosDecreasedEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\LoyaltyKudos\LoyaltyKudosDecreased.cshtml");

        public string CommitteeSuggestionEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Committees\CommitteesSuggestion.cshtml");

        public string ServiceRequestCommentEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\ServiceRequests\ServiceRequestComment.cshtml");

        public string ServiceRequestEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\ServiceRequests\NewServiceRequest.cshtml");

        public string ServiceRequestUpdateEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\ServiceRequests\UpdateServiceRequest.cshtml");

        public string StartedLotteryEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Lotteries\StartedLottery.cshtml");

        public string NewMentionTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Wall\NewMention.cshtml");

        public string KudosSentEmailTemplateAbsolutePath => GetAbsolutePath(@"EmailTemplates\Kudos\KudosSent.cshtml");

        public virtual string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        private string GetAbsolutePath(string relativePath) => Path.Combine(BaseDirectory, relativePath);
    }
}
