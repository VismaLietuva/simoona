namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IEmailTemplateConfiguration
    {
        string NewMentionTemplateAbsolutePath { get; }

        string KudosSentEmailTemplateAbsolutePath { get; }

        string LayoutEmailTemplateAbsolutePath { get; }

        string NewWallPostEmailTemplateAbsolutePath { get; }

        string KudosRejectedEmailTemplateAbsolutePath { get; }

        string KudosReceivedEmailTemplateAbsolutePath { get; }

        string KudosDecreasedEmailTemplateAbsolutePath { get; }

        string BirthdaysNotificationAbsolutePath { get; }

        string UserConfirmationEmailTemplateAbsolutePath { get; }

        string NotificationAboutNewUserEmailTemplateAbsolutePath { get; }

        string NewCommentEmailTemplateAbsolutePath { get; }

        string ResetPasswordTemplateAbsolutePath { get; }

        string VerifyEmailTemplateAbsolutePath { get; }

        // Premium
        string BookReminderEmailTemplateAbsolutePath { get; }

        string BookReportEmailTemplateAbsolutePath { get; }

        string EventParticipantExpelledEmailTemplateAbsolutePath { get; }

        string EventJoinRemindEmailTemplateAbsolutePath { get; }

        string CoacheeJoinedEventEmailTemplateAbsolutePath { get; }

        string CoacheeLeftEventEmailTemplateAbsolutePath { get; }

        string BookTakenEmailTemplateAbsolutePath { get; }

        string LoyaltyKudosReceivedEmailTemplateAbsolutePath { get; }

        string LoyaltyKudosDecreasedEmailTemplateAbsolutePath { get; }

        string CommitteeSuggestionEmailTemplateAbsolutePath { get; }

        string ServiceRequestCommentEmailTemplateAbsolutePath { get; }

        string ServiceRequestEmailTemplateAbsolutePath { get; }

        string ServiceRequestUpdateEmailTemplateAbsolutePath { get; }

        string StartedLotteryEmailTemplateAbsolutePath { get; }

        string BaseDirectory { get; }
    }
}
