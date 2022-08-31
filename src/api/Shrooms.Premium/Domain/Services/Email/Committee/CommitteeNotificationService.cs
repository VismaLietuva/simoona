using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using CommitteeEntity = Shrooms.DataLayer.EntityModels.Models.Committee.Committee;

namespace Shrooms.Premium.Domain.Services.Email.Committee
{
    public class CommitteeNotificationService : ICommitteeNotificationService
    {
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IMailTemplate _mailTemplate;
        private readonly IApplicationSettings _appSettings;
        private readonly IMailingService _mailingService;
        private readonly IDbSet<CommitteeEntity> _committeeDbSet;
        private readonly DbSet<CommitteeSuggestion> _suggestionDbSet;

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

        public async Task NotifyCommitteeMembersAboutNewSuggestionAsync(CommitteeSuggestionCreatedDto createdDto)
        {
            var committee = await _committeeDbSet.Include(inc => inc.Members).FirstOrDefaultAsync(wh => wh.Id == createdDto.CommitteeId);
            if (committee == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.SuggestionCommiteNotFound);
            }

            var suggestion = await _suggestionDbSet.FindAsync(createdDto.SuggestionId);
            if (suggestion == null)
            {
                throw new ServiceException($"Suggestion {createdDto.SuggestionId} for committee {createdDto.CommitteeId} not found");
            }

            await NotifyCommitteeMembersAboutNewSuggestionAsync(committee, suggestion);
        }

        private async Task NotifyCommitteeMembersAboutNewSuggestionAsync(CommitteeEntity committee, CommitteeSuggestion suggestion)
        {
            if (committee.Members == null || !committee.Members.Any())
            {
                return;
            }

            IList<string> membersEmails = committee.Members.Select(s => s.Email).ToList();

            var organizationName = await _organizationDbSet
                .Where(organization => organization.Id == committee.OrganizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();

            var committeesListUrl = _appSettings.CommitteeSugestionUrl(organizationName);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var subject = string.Format(Resources.Common.CommitteeSuggestionEmailSubject, committee.Name);

            var emailTemplateViewModel = new CommitteeSuggestionEmailTemplateViewModel(userNotificationSettingsUrl,
                committee.Name,
                suggestion.Title,
                suggestion.Description,
                committeesListUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel);

            await _mailingService.SendEmailAsync(new EmailDto(membersEmails, subject, body));
        }
    }
}
