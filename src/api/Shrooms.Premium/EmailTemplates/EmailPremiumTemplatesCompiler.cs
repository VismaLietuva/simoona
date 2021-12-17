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
            _baseDir = Path.Combine(baseDir, "Extensions");

            AddAndCompile(EmailPremiumTemplateCacheKeys.BookRemind, @"EmailTemplates\Books\BookRemind.cshtml", typeof(BookReminderEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookReport, @"EmailTemplates\Books\BookReport.cshtml", typeof(BookReportEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventParticipantExpelled, @"EmailTemplates\Events\ParticipantExpelled.cshtml", typeof(EventParticipantExpelledEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.EventJoinRemind, @"EmailTemplates\Events\RemindToJoin.cshtml", typeof(EventJoinRemindEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.BookTaken, @"EmailTemplates\Books\BookTaken.cshtml", typeof(BookTakenEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived, @"EmailTemplates\LoyaltyKudos\LoyaltyKudosReceived.cshtml", typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.LoyaltyKudosDecreased, @"EmailTemplates\LoyaltyKudos\LoyaltyKudosDecreased.cshtml", typeof(LoyaltyKudosReceivedDecreasedEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.CommitteeSuggestion, @"EmailTemplates\Committees\CommitteesSuggestion.cshtml", typeof(CommitteeSuggestionEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestComment, @"EmailTemplates\ServiceRequests\ServiceRequestComment.cshtml", typeof(ServiceRequestCommentEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequest, @"EmailTemplates\ServiceRequests\NewServiceRequest.cshtml", typeof(ServiceRequestEmailTemplateViewModel));
            AddAndCompile(EmailPremiumTemplateCacheKeys.ServiceRequestUpdate, @"EmailTemplates\ServiceRequests\UpdateServiceRequest.cshtml", typeof(ServiceRequestUpdateEmailTemplateViewModel));
        }

        private static void AddAndCompile(string templateKey, string relativePath, Type templateViewModel)
        {
            var absolutePath = Path.Combine(_baseDir, relativePath);

            Engine.Razor.AddTemplate(templateKey, File.ReadAllText(absolutePath));
            Engine.Razor.Compile(templateKey, templateViewModel);
        }
    }
}