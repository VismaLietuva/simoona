using System;
using System.Collections.Generic;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;

namespace Shrooms.EmailTemplates.Seeds
{
    // Sample view models for every template. Must stay deterministic - no DateTime.Now.
    public static class EmailTemplateSeeds
    {
        public const string ShowcaseKey = "/EmailTemplates/Design/Showcase.cshtml";

        private const string SettingsUrl = "https://simoona.example.com/settings/notifications";
        private const string HomeUrl = "https://simoona.example.com/";
        private const string PictureUrl = "https://simoona.example.com/pictures/avatar.png";
        private static readonly DateTime SampleDate = new DateTime(2026, 3, 17, 9, 30, 0, DateTimeKind.Utc);
        private static readonly DateTime SampleEndDate = new DateTime(2026, 3, 17, 17, 0, 0, DateTimeKind.Utc);

        public static IReadOnlyList<EmailTemplateSeed> All { get; } = BuildAll();

        private static IReadOnlyList<EmailTemplateSeed> BuildAll()
        {
            var seeds = new List<EmailTemplateSeed>
            {
                // Core
                new(EmailTemplateCacheKeys.NewWallPost, new NewWallPostEmailTemplateViewModel(
                    "Engineering wall", PictureUrl, "Rasa Petraitiene", "https://simoona.example.com/posts/1",
                    "We are shipping the new email templates this week.", SettingsUrl, "Read it")),
                new(EmailTemplateCacheKeys.NewPostComment, new NewCommentEmailTemplateViewModel(
                    "Quarterly planning", PictureUrl, "Rasa Petraitiene", "https://simoona.example.com/posts/1#c2",
                    "Agreed - let us pick this up on Monday.", SettingsUrl, "View comment")),
                new(EmailTemplateCacheKeys.NewMention, new NewMentionTemplateViewModel(
                    "You were mentioned", "Tomas Kazlauskas", "Rasa Petraitiene",
                    "https://simoona.example.com/posts/1", SettingsUrl,
                    "@Tomas Kazlauskas could you review this before the release?")),
                new(EmailTemplateCacheKeys.BirthdaysNotification, new BirthdaysNotificationTemplateViewModel(
                    new List<BirthdaysNotificationEmployeeViewModel>
                    {
                        new() { FullName = "Rasa Petraitiene", PictureUrl = PictureUrl, ProfileUrl = "https://simoona.example.com/profile/1" },
                        new() { FullName = "Tomas Kazlauskas", PictureUrl = PictureUrl, ProfileUrl = "https://simoona.example.com/profile/2" }
                    },
                    SettingsUrl)),
                new(EmailTemplateCacheKeys.KudosSent, new KudosSentEmailTemplateViewModel(
                    SettingsUrl, "Rasa Petraitiene", 25, "Thanks for covering the on-call shift.",
                    "https://simoona.example.com/kudos/profile/1")),
                new(EmailTemplateCacheKeys.KudosReceived, new KudosReceivedDecreasedEmailTemplateViewModel(
                    SettingsUrl, 25, "Send a Smile", "Rasa Petraitiene", "Thanks for covering the on-call shift.",
                    "https://simoona.example.com/kudos/profile/1")),
                new(EmailTemplateCacheKeys.KudosDecreased, new KudosReceivedDecreasedEmailTemplateViewModel(
                    SettingsUrl, 15, "Minus Kudos", "Rasa Petraitiene", "Correcting a duplicate award.",
                    "https://simoona.example.com/kudos/profile/1")),
                new(EmailTemplateCacheKeys.KudosRejected, new KudosRejectedEmailTemplateViewModel(
                    SettingsUrl, "Tomas Kazlauskas", 25, "Send a Smile", "Thanks for covering the on-call shift.",
                    "Kudos type does not match the described work.", "https://simoona.example.com/kudos/profile/1")),
                new(EmailTemplateCacheKeys.UserConfirmation, new UserConfirmationEmailTemplateViewModel(
                    SettingsUrl, "https://simoona.example.com",
                    "<p>Welcome to Simoona. Your account is ready.</p>")),
                new(EmailTemplateCacheKeys.NotificationAboutNewUser, new NotificationAboutNewUserEmailTemplateViewModel(
                    SettingsUrl, "https://simoona.example.com/profile/3", "Tomas Kazlauskas")),
                new(EmailTemplateCacheKeys.ResetPassword, new ResetPasswordTemplateViewModel(
                    "Tomas Kazlauskas", SettingsUrl, "https://simoona.example.com/reset?token=sample")),
                new(EmailTemplateCacheKeys.VerifyEmail, new VerifyEmailTemplateViewModel(
                    "Tomas Kazlauskas", SettingsUrl, "https://simoona.example.com/verify?token=sample")),

                // Premium
                new(EmailPremiumTemplateCacheKeys.BookTaken, new BookTakenEmailTemplateViewModel(
                    SettingsUrl, "Refactoring", "Martin Fowler", "https://simoona.example.com/books/1")),
                new(EmailPremiumTemplateCacheKeys.BookRemind, new BookReminderEmailTemplateViewModel(
                    "Refactoring", "Martin Fowler", "2026-03-01", "https://simoona.example.com/books/1",
                    "Tomas Kazlauskas", SettingsUrl)),
                new(EmailPremiumTemplateCacheKeys.BookReport, new BookReportEmailTemplateViewModel(
                    "Refactoring", "Martin Fowler", "Book is missing", "Last seen in the Vilnius office.",
                    "https://simoona.example.com/books/1", "Tomas Kazlauskas", SettingsUrl)),
                new(EmailPremiumTemplateCacheKeys.CommitteeSuggestion, new CommitteeSuggestionEmailTemplateViewModel(
                    SettingsUrl, "Culture committee", "Quarterly team lunch",
                    "Rotating the venue each quarter so every team gets a turn.",
                    "https://simoona.example.com/committees/1")),
                new(EmailPremiumTemplateCacheKeys.EventNew, new NewEventEmailTemplateViewModel(
                    "https://simoona.example.com/events/1", "Engineering all-hands",
                    "<p>Roadmap review followed by open questions.</p>", "Vilnius HQ, 4th floor",
                    SampleDate, SettingsUrl)),
                new(EmailPremiumTemplateCacheKeys.EventShared, new SharedEventEmailTemplateViewModel(
                    "https://simoona.example.com/posts/9", "https://simoona.example.com/events/1",
                    "Rasa Petraitiene", "<p>Worth attending if you work on the API.</p>", "Engineering wall",
                    "Engineering all-hands", SampleDate, "<p>Roadmap review followed by open questions.</p>",
                    "Vilnius HQ, 4th floor", SettingsUrl)),
                new(EmailPremiumTemplateCacheKeys.EventStartRemind, new EventReminderStartEmailTemplateViewModel(
                    SettingsUrl, "Engineering all-hands", "https://simoona.example.com/events/1", SampleDate)),
                new(EmailPremiumTemplateCacheKeys.EventDeadlineRemind, new EventReminderDeadlineEmailTemplateViewModel(
                    SettingsUrl, "Engineering all-hands", "https://simoona.example.com/events/1",
                    SampleDate, SampleEndDate)),
                new(EmailPremiumTemplateCacheKeys.EventParticipantExpelled, new EventParticipantExpelledEmailTemplateViewModel(
                    SettingsUrl, "Engineering all-hands", "https://simoona.example.com/events/1")),
                new(EmailPremiumTemplateCacheKeys.EventJoinRemind, new EventJoinRemindEmailTemplateViewModel(SettingsUrl)
                {
                    EventTypes = new Dictionary<string, string>
                    {
                        { "Team building", "https://simoona.example.com/events?type=team-building" },
                        { "Workshops", "https://simoona.example.com/events?type=workshops" }
                    }
                }),
                new(EmailPremiumTemplateCacheKeys.CoacheeJoinedEvent, new CoacheeJoinedEventEmailTemplateViewModel(
                    SettingsUrl, SampleAttendStatus(), "https://simoona.example.com/events/1")),
                new(EmailPremiumTemplateCacheKeys.CoacheeLeftEvent, new CoacheeLeftEventEmailTemplateViewModel(
                    SettingsUrl, SampleAttendStatus(), "https://simoona.example.com/events/1")),
                new(EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived, new LoyaltyKudosReceivedDecreasedEmailTemplateViewModel(
                    SettingsUrl, 50, "Loyalty Kudos", "Simoona", "Two years with the company.",
                    "https://simoona.example.com/kudos/profile/1")),
                new(EmailPremiumTemplateCacheKeys.LoyaltyKudosDecreased, new LoyaltyKudosReceivedDecreasedEmailTemplateViewModel(
                    SettingsUrl, 20, "Loyalty Kudos", "Simoona", "Correcting a duplicate award.",
                    "https://simoona.example.com/kudos/profile/1")),
                new(EmailPremiumTemplateCacheKeys.ServiceRequest, new ServiceRequestEmailTemplateViewModel(
                    SettingsUrl, "Broken monitor at desk 14", "Tomas Kazlauskas",
                    "https://simoona.example.com/service-requests/1")),
                new(EmailPremiumTemplateCacheKeys.ServiceRequestComment, new ServiceRequestCommentEmailTemplateViewModel(
                    SettingsUrl, "Broken monitor at desk 14", "Rasa Petraitiene",
                    "A replacement is on the way.", "https://simoona.example.com/service-requests/1")),
                new(EmailPremiumTemplateCacheKeys.ServiceRequestUpdate, new ServiceRequestUpdateEmailTemplateViewModel(
                    SettingsUrl, "Broken monitor at desk 14", "Rasa Petraitiene", "Done",
                    "https://simoona.example.com/service-requests/1")),
                new(EmailPremiumTemplateCacheKeys.StartedLottery, new StartedLotteryEmailTemplateViewModel(
                    new LotteryStartedEmailDto
                    {
                        Id = 1,
                        Title = "Concert tickets",
                        Description = "<p>Two tickets to the summer festival.</p>",
                        EntryFee = 20,
                        EndDate = SampleEndDate
                    },
                    "https://simoona.example.com/lotteries/1", SampleEndDate, SettingsUrl)),
                new(EmailPremiumTemplateCacheKeys.LotteryTicketGifted, new LotteryTicketGiftedEmailTemplateViewModel(
                    "Concert tickets", "https://simoona.example.com/lotteries/1", "Rasa Petraitiene", 3, SettingsUrl)),

                new(ShowcaseKey, new KudosSentEmailTemplateViewModel(SettingsUrl, "Rasa Petraitiene", 25, "Sample", "https://simoona.example.com"))
            };

            // MailTemplate fills this from configuration; renders that bypass it need it too.
            seeds.ForEach(seed => seed.Model.HomeUrl = HomeUrl);

            return seeds;
        }

        private static UserEventAttendStatusChangeEmailDto SampleAttendStatus()
        {
            return new UserEventAttendStatusChangeEmailDto
            {
                FirstName = "Tomas",
                LastName = "Kazlauskas",
                OrganizationId = 1,
                ManagerId = "manager-1",
                ManagerEmail = "manager@simoona.example.com",
                EventName = "Engineering all-hands",
                EventId = new Guid("00000000-0000-0000-0000-000000000001"),
                EventStartDate = SampleDate,
                EventEndDate = SampleEndDate
            };
        }
    }

    public record EmailTemplateSeed(string Key, BaseEmailTemplateViewModel Model);
}
