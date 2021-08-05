using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Domain.Services.Email.Committee;

namespace Shrooms.Premium.Domain.Services.Committees
{
    public class CommitteesService : ICommitteesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWork2 _uow;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Committee> _committeeDbSet;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<Committee> _committeeRepository;
        private readonly IMapper _mapper;

        public CommitteesService(IMapper mapper, IUnitOfWork unitOfWork, IUnitOfWork2 uow, IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _applicationUserRepository = _unitOfWork.GetRepository<ApplicationUser>();
            _committeeRepository = _unitOfWork.GetRepository<Committee>();
            _uow = uow;
            _asyncRunner = asyncRunner;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _committeeDbSet = uow.GetDbSet<Committee>();
        }

        #region private methods

        private async Task<Committee> CreateCommitteeModelAsync(CommitteePostDto committeePostDto, Committee committeeModel = null)
        {
            committeeModel = _mapper.Map(committeePostDto, committeeModel) ?? _mapper.Map<CommitteePostDto, Committee>(committeePostDto);
            await AddRemoveCommitteeMembersAsync(committeePostDto, committeeModel);

            if (committeeModel != null)
            {
                await UpdateCommitteeLeadsAsync(committeePostDto, committeeModel);
                await UpdateCommitteeDelegatesAsync(committeePostDto, committeeModel);
            }

            return committeeModel;
        }

        private async Task UpdateCommitteeLeadsAsync(CommitteePostDto committeePostDto, Committee committee)
        {
            var newLeadsIds = committeePostDto.Leads.Select(x => x.Id).ToList();
            var newLeads = await _usersDbSet.Where(x => newLeadsIds.Contains(x.Id)).ToListAsync();
            committee.Leads = newLeads;
        }

        private async Task UpdateCommitteeDelegatesAsync(CommitteePostDto committeePostDto, Committee committee)
        {
            var newDelegatesIds = committeePostDto.Delegates.Select(x => x.Id).ToList();
            var newDelegates = await _usersDbSet.Where(x => newDelegatesIds.Contains(x.Id)).ToListAsync();
            committee.Delegates = newDelegates;
        }

        private async Task AddRemoveCommitteeMembersAsync(CommitteePostDto committeePostDto, Committee committeeModel)
        {
            var membersInModelIds = _mapper.Map<IEnumerable<ApplicationUserMinimalDto>, string[]>(committeePostDto.Members);
            if (committeeModel.Members != null)
            {
                var membersInCommitteeIds = _mapper.Map<IEnumerable<ApplicationUser>, string[]>(committeeModel.Members);

                var membersToAdd = _applicationUserRepository.Get(u => membersInModelIds.Contains(u.Id) && !membersInCommitteeIds.Contains(u.Id));
                await membersToAdd.ForEachAsync(m => committeeModel.Members.Add(m));

                var membersToRemove = _applicationUserRepository.Get(u => !membersInModelIds.Contains(u.Id) && membersInCommitteeIds.Contains(u.Id));
                await membersToRemove.ForEachAsync(m => committeeModel.Members.Remove(m));
            }
            else
            {
                var membersToAdd = _applicationUserRepository.Get(u => membersInModelIds.Contains(u.Id));
                committeeModel.Members = await membersToAdd.ToListAsync();
            }
        }

        private async Task ChangeKudosCommitteeAsync(CommitteePostDto committeePostDto)
        {
            if (!committeePostDto.IsKudosCommittee)
            {
                return;
            }

            var oldCommittee = await _committeeRepository.Get(k => k.IsKudosCommittee).FirstOrDefaultAsync();

            if (oldCommittee == null)
            {
                return;
            }

            if (oldCommittee.Id == committeePostDto.Id)
            {
                return;
            }

            oldCommittee.IsKudosCommittee = false;
            _committeeRepository.Update(oldCommittee);

            await _unitOfWork.SaveAsync();
        }

        #endregion

        public async Task PutCommitteeAsync(CommitteePostDto committeePostDto)
        {
            var committeeModel = await _committeeRepository
                .Get(c => c.Id == committeePostDto.Id, includeProperties: "Members,Leads,Delegates")
                .FirstOrDefaultAsync();

            if (committeeModel == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.ModelNotFoundException);
            }

            await ChangeKudosCommitteeAsync(committeePostDto);

            await CreateCommitteeModelAsync(committeePostDto, committeeModel);
            _committeeRepository.Update(committeeModel);

            await _unitOfWork.SaveAsync();
        }

        public async Task PostCommitteeAsync(CommitteePostDto committeePostDto)
        {
            if (await _committeeRepository.GetByIdAsync(committeePostDto.Id) != null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.CommitteeIdException);
            }

            var committeeModel = await CreateCommitteeModelAsync(committeePostDto);
            await ChangeKudosCommitteeAsync(committeePostDto);
            _committeeRepository.Insert(committeeModel);

            await _unitOfWork.SaveAsync();
        }

        public async Task PostSuggestionAsync(CommitteeSuggestionPostDto dto, string userId)
        {
            var committee = await _committeeRepository
                .Get(c => c.Id == dto.CommitteeId, includeProperties: "Suggestions, Members")
                .FirstOrDefaultAsync();

            if (committee == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.SuggestionCommiteNotFound);
            }

            var suggestion = _mapper.Map<CommitteeSuggestion>(dto);

            suggestion.User = await _applicationUserRepository.GetByIdAsync(userId);
            suggestion.Date = DateTime.UtcNow;

            committee.Suggestions.Add(suggestion);

            _committeeRepository.Update(committee);
            await _unitOfWork.SaveAsync();

            var suggestionDto = new CommitteeSuggestionCreatedDto
            {
                CommitteeId = committee.Id,
                SuggestionId = suggestion.Id
            };

            _asyncRunner.Run<ICommitteeNotificationService>(async notifier => await notifier.NotifyCommitteeMembersAboutNewSuggestionAsync(suggestionDto), _uow.ConnectionName);
        }

        public async Task<IEnumerable<CommitteeSuggestionDto>> GetCommitteeSuggestionsAsync(int id)
        {
            var committee = await _committeeRepository.Get(c => c.Id == id, includeProperties: "Suggestions.User").FirstOrDefaultAsync();

            var committeeSuggestions = committee?.Suggestions.OrderByDescending(d => d.Date);

            return _mapper.Map<IEnumerable<CommitteeSuggestionDto>>(committeeSuggestions);
        }

        public async Task DeleteCommitteeSuggestionAsync(int committeeId, int suggestionId, UserAndOrganizationDto userAndOrg)
        {
            var committee = await _committeeDbSet
                .Include(u => u.Suggestions)
                .FirstOrDefaultAsync(u => u.Id == committeeId);

            if (committee == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Committee does not exist");
            }

            var suggestion = committee.Suggestions.FirstOrDefault(x => x.Id == suggestionId);

            committee.Suggestions.Remove(suggestion);

            await _uow.SaveChangesAsync(userAndOrg.UserId);
        }

        public async Task<CommitteeViewDto> GetKudosCommitteeAsync()
        {
            var kudosCommittee = await _committeeRepository.Get(k => k.IsKudosCommittee, includeProperties: "Members").FirstOrDefaultAsync();

            var kudosCommitteeMapped = _mapper.Map<CommitteeViewDto>(kudosCommittee);

            return kudosCommitteeMapped;
        }

        public async Task<int> GetKudosCommitteeIdAsync()
        {
            var kudosCommittee = await _committeeRepository.Get(k => k.IsKudosCommittee).FirstOrDefaultAsync();

            if (kudosCommittee == null)
            {
                return 0;
            }

            return kudosCommittee.Id;
        }
    }
}
