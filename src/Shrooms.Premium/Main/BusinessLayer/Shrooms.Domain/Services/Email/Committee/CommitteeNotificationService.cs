using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Committees;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System;
using Shrooms.Domain.ServiceExceptions;

namespace Shrooms.Domain.Services.Email.Committee
{
    public class CommitteeNotificationService : ICommitteeNotificationService
    {
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IMailTemplate _mailTemplate;
        private readonly IApplicationSettings _appSettings;
        private readonly IMailingService _mailingService;
        private readonly IDbSet<EntityModels.Models.Committee> _committeeDbSet;
        private readonly IDbSet<CommitteeSuggestion> _suggestionDbSet;

        public CommitteeNotificationService(UnitOfWork2 uow,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings,
            IMailingService mailingService)
        {
            _organizationDbSet = uow.GetDbSet<Organization>();
            _mailTemplate = mailTemplate;
            _appSettings = appSettings;
            _mailingService = mailingService;
            _committeeDbSet = uow.GetDbSet<EntityModels.Models.Committee>();
            _suggestionDbSet = uow.GetDbSet<CommitteeSuggestion>();
        }

        public void NotifyCommitteeMembersAboutNewSuggestion(CommitteeSuggestionCreatedDto createdDto)
        {
            var committee = _committeeDbSet.Include(inc => inc.Members).Where(wh => wh.Id == createdDto.CommitteeId).FirstOrDefault();
            if (committee == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.SuggestionCommiteNotFound);
            }
            var suggestion = _suggestionDbSet.Find(createdDto.SuggestionId);
            if(suggestion==null)
            {
                throw new ServiceException($"Suggestion {createdDto.SuggestionId} for committee {createdDto.CommitteeId} not found");
            }
            NotifyCommitteeMembersAboutNewSuggestion(committee, suggestion);
        }

        private void NotifyCommitteeMembersAboutNewSuggestion(EntityModels.Models.Committee committee, CommitteeSuggestion suggestion)
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
