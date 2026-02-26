using System;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.Premium.EmailTemplates
{
    public class EmailPremiumTemplatesCompiler : IEmailTemplateCompiler
    {
        private static string _baseDir;

        public void Register(string baseDir)
        {
            _baseDir = baseDir;

            AddAndCompile(EmailPremiumTemplateCacheKeys.BookRemind, Path.Combine("EmailTemplates", "Books", "BookRemind.html"), typeof(BookReminderEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookReport, Path.Combine("EmailTemplates", "Books", "BookReport.html"), typeof(BookReportEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventParticipantExpelled, Path.Combine("EmailTemplates", "Events", "ParticipantExpelled.html"), typeof(EventParticipantExpelledEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventJoinRemind, Path.Combine("EmailTemplates", "Events", "RemindToJoin.html"), typeof(EventJoinRemindEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventStartRemind, Path.Combine("EmailTemplates", "Events", "RemindStartDate.html"), typeof(EventReminderStartEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventDeadlineRemind, Path.Combine("EmailTemplates", "Events", "RemindDeadlineDate.html"), typeof(EventReminderDeadlineEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CoacheeJoinedEvent, Path.Combine("EmailTemplates", "Events", "CoacheeJoinedEvent.html"), typeof(CoacheeJoinedEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CoacheeLeftEvent, Path.Combine("EmailTemplates", "Events", "CoacheeLeftEvent.html"), typeof(CoacheeLeftEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookTaken, Path.Combine("EmailTemplates", "Books", "BookTaken.html"), typeof(BookTakenEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived, Path.Combine("EmailTemplates", "LoyaltyKudos", "LoyaltyKudosReceived.html"), typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosDecreased, Path.Combine("EmailTemplates", "LoyaltyKudos", "LoyaltyKudosDecreased.html"), typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CommitteeSuggestion, Path.Combine("EmailTemplates", "Committees", "CommitteesSuggestion.html"), typeof(CommitteeSuggestionEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestComment, Path.Combine("EmailTemplates", "ServiceRequests", "ServiceRequestComment.html"), typeof(ServiceRequestCommentEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequest, Path.Combine("EmailTemplates", "ServiceRequests", "NewServiceRequest.html"), typeof(ServiceRequestEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestUpdate, Path.Combine("EmailTemplates", "ServiceRequests", "UpdateServiceRequest.html"), typeof(ServiceRequestUpdateEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.StartedLottery, Path.Combine("EmailTemplates", "Lotteries", "StartedLottery.html"), typeof(StartedLotteryEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LotteryTicketGifted, Path.Combine("EmailTemplates", "Lotteries", "LotteryTicketGifted.html"), typeof(LotteryTicketGiftedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventShared, Path.Combine("EmailTemplates", "Events", "SharedEvent.html"), typeof(SharedEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventNew, Path.Combine("EmailTemplates", "Events", "NewEvent.html"), typeof(NewEventEmailTemplateViewModel));
        }

        private static void AddAndCompile(string templateKey, string relativePath, Type templateViewModel)
        {
            var absolutePath = Path.Combine(_baseDir, relativePath);

            Engine.Razor.AddTemplate(templateKey, File.ReadAllText(absolutePath));
            Engine.Razor.Compile(templateKey, templateViewModel);
        }
    }
}
