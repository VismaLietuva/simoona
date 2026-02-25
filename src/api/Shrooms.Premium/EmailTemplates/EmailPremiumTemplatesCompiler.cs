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

            AddAndCompile(EmailPremiumTemplateCacheKeys.BookRemind, Path.Combine("EmailTemplates", "Books", "BookRemind.cshtml"), typeof(BookReminderEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookReport, Path.Combine("EmailTemplates", "Books", "BookReport.cshtml"), typeof(BookReportEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventParticipantExpelled, Path.Combine("EmailTemplates", "Events", "ParticipantExpelled.cshtml"), typeof(EventParticipantExpelledEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventJoinRemind, Path.Combine("EmailTemplates", "Events", "RemindToJoin.cshtml"), typeof(EventJoinRemindEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventStartRemind, Path.Combine("EmailTemplates", "Events", "RemindStartDate.cshtml"), typeof(EventReminderStartEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventDeadlineRemind, Path.Combine("EmailTemplates", "Events", "RemindDeadlineDate.cshtml"), typeof(EventReminderDeadlineEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CoacheeJoinedEvent, Path.Combine("EmailTemplates", "Events", "CoacheeJoinedEvent.cshtml"), typeof(CoacheeJoinedEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CoacheeLeftEvent, Path.Combine("EmailTemplates", "Events", "CoacheeLeftEvent.cshtml"), typeof(CoacheeLeftEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookTaken, Path.Combine("EmailTemplates", "Books", "BookTaken.cshtml"), typeof(BookTakenEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived, Path.Combine("EmailTemplates", "LoyaltyKudos", "LoyaltyKudosReceived.cshtml"), typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosDecreased, Path.Combine("EmailTemplates", "LoyaltyKudos", "LoyaltyKudosDecreased.cshtml"), typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CommitteeSuggestion, Path.Combine("EmailTemplates", "Committees", "CommitteesSuggestion.cshtml"), typeof(CommitteeSuggestionEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestComment, Path.Combine("EmailTemplates", "ServiceRequests", "ServiceRequestComment.cshtml"), typeof(ServiceRequestCommentEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequest, Path.Combine("EmailTemplates", "ServiceRequests", "NewServiceRequest.cshtml"), typeof(ServiceRequestEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestUpdate, Path.Combine("EmailTemplates", "ServiceRequests", "UpdateServiceRequest.cshtml"), typeof(ServiceRequestUpdateEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.StartedLottery, Path.Combine("EmailTemplates", "Lotteries", "StartedLottery.cshtml"), typeof(StartedLotteryEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LotteryTicketGifted, Path.Combine("EmailTemplates", "Lotteries", "LotteryTicketGifted.cshtml"), typeof(LotteryTicketGiftedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventShared, Path.Combine("EmailTemplates", "Events", "SharedEvent.cshtml"), typeof(SharedEventEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventNew, Path.Combine("EmailTemplates", "Events", "NewEvent.cshtml"), typeof(NewEventEmailTemplateViewModel));
        }

        private static void AddAndCompile(string templateKey, string relativePath, Type templateViewModel)
        {
            var absolutePath = Path.Combine(_baseDir, relativePath);

            Engine.Razor.AddTemplate(templateKey, File.ReadAllText(absolutePath));
            Engine.Razor.Compile(templateKey, templateViewModel);
        }
    }
}
