using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Constants;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Committee;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Committee
{
    public class CommitteeNotificationService : ICommitteeNotificationService
    {
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IMailTemplate _mailTemplate;
        private readonly IApplicationSettings _appSettings;
        private readonly IMailingService _mailingService;

        public CommitteeNotificationService(IUnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings,
            IMailingService mailingService)
        {
            _organizationDbSet = uow.GetDbSet<Organization>();
            _mailTemplate = mailTemplate;
            _appSettings = appSettings;
            _mailingService = mailingService;
        }

        public void NotifyCommitteeMembersAboutNewSuggestion(EntityModels.Models.Committee.Committee committee, CommitteeSuggestion suggestion)
        {
            if (committee.Members != null && committee.Members.Any())
            {
                IList<string> membersEmails = committee.Members.Select(s => s.Email).ToList();

                var organizationName = _organizationDbSet
                    .Where(organization => organization.Id == committee.OrganizationId)
                    .Select(organization => organization.ShortName)
                    .FirstOrDefault();

                var committeesListUrl = _appSettings.CommitteeSugestionUrl(organizationName);
                var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
                var subject = string.Format(Resources.Common.CommitteeSuggestionEmailSubject, committee.Name);

                var emailTemplateViewModel = new CommitteeSuggestionEmailTemplateViewModel(
                    userNotificationSettingsUrl,
                    committee.Name,
                    suggestion.Title,
                    suggestion.Description,
                    committeesListUrl);

                var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.CommitteeSuggestion);

                _mailingService.SendEmail(new EmailDto(membersEmails, subject, body));
            }
        }
    }
}
