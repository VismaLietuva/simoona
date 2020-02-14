using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Committees;
using CommitteeEntity = Shrooms.DataLayer.EntityModels.Models.Committee.Committee;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Committee
{
    public class CommitteeNotificationService : ICommitteeNotificationService
    {
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IMailTemplate _mailTemplate;
        private readonly IApplicationSettings _appSettings;
        private readonly IMailingService _mailingService;
        private readonly IDbSet<CommitteeEntity> _committeeDbSet;
        private readonly IDbSet<CommitteeSuggestion> _suggestionDbSet;

        public CommitteeNotificationService(IUnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings,
            IMailingService mailingService)
        {
            _organizationDbSet = uow.GetDbSet<Organization>();
            _mailTemplate = mailTemplate;
            _appSettings = appSettings;
            _mailingService = mailingService;
            _committeeDbSet = uow.GetDbSet<CommitteeEntity>();
            _suggestionDbSet = uow.GetDbSet<CommitteeSuggestion>();
        }

        public void NotifyCommitteeMembersAboutNewSuggestion(CommitteeSuggestionCreatedDto createdDto)
        {
            var committee = _committeeDbSet.Include(inc => inc.Members).FirstOrDefault(wh => wh.Id == createdDto.CommitteeId);
            if (committee == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.SuggestionCommiteNotFound);
            }

            var suggestion = _suggestionDbSet.Find(createdDto.SuggestionId);
            if (suggestion == null)
            {
                throw new ServiceException($"Suggestion {createdDto.SuggestionId} for committee {createdDto.CommitteeId} not found");
            }

            NotifyCommitteeMembersAboutNewSuggestion(committee, suggestion);
        }

        private void NotifyCommitteeMembersAboutNewSuggestion(CommitteeEntity committee, CommitteeSuggestion suggestion)
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
