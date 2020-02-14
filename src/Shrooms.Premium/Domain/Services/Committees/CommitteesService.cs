using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Infrastructure.FireAndForget;
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
        private readonly IDbSet<Committee> _committteeDbSet;
        private readonly IRepository<ApplicationUser> _applicationUserRepository;
        private readonly IRepository<Committee> _committeeRepository;
        private readonly IMapper _mapper;

        public CommitteesService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IUnitOfWork2 uow,
            IAsyncRunner asyncRunner)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _applicationUserRepository = _unitOfWork.GetRepository<ApplicationUser>();
            _committeeRepository = _unitOfWork.GetRepository<Committee>();
            _uow = uow;
            _asyncRunner = asyncRunner;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _committteeDbSet = uow.GetDbSet<Committee>();
        }

        #region private methods

        private Committee CreateCommitteeModel(CommitteePostDTO modelDTO, Committee committeeModel = null)
        {
            committeeModel = _mapper.Map(modelDTO, committeeModel) ?? _mapper.Map<CommitteePostDTO, Committee>(modelDTO);
            AddRemoveCommitteeMembers(modelDTO, committeeModel);
            if (committeeModel != null)
            {
                UpdateCommitteeLeads(modelDTO, committeeModel);
                UpdateCommitteeDelegates(modelDTO, committeeModel);
            }

            return committeeModel;
        }

        private void UpdateCommitteeLeads(CommitteePostDTO modelDto, Committee committee)
        {
            var newLeadsIds = modelDto.Leads.Select(x => x.Id).ToList();
            var newLeads = _usersDbSet.Where(x => newLeadsIds.Contains(x.Id)).ToList();
            committee.Leads = newLeads;
        }

        private void UpdateCommitteeDelegates(CommitteePostDTO modelDto, Committee committee)
        {
            var newDelegatesIds = modelDto.Delegates.Select(x => x.Id).ToList();
            var newDelegates = _usersDbSet.Where(x => newDelegatesIds.Contains(x.Id)).ToList();
            committee.Delegates = newDelegates;
        }

        private void AddRemoveCommitteeMembers(CommitteePostDTO modelDTO, Committee committeeModel)
        {
            var membersInModelIds = _mapper.Map<IEnumerable<ApplicationUser>, string[]>(modelDTO.Members);
            if (committeeModel.Members != null)
            {
                var membersInCommitteeIds = _mapper.Map<IEnumerable<ApplicationUser>, string[]>(committeeModel.Members);

                var membersToAdd = _applicationUserRepository.Get(u => membersInModelIds.Contains(u.Id) && !membersInCommitteeIds.Contains(u.Id));
                membersToAdd.ForEach(m => committeeModel.Members.Add(m));

                var membersToRemove = _applicationUserRepository.Get(u => !membersInModelIds.Contains(u.Id) && membersInCommitteeIds.Contains(u.Id));
                membersToRemove.ForEach(m => committeeModel.Members.Remove(m));
            }
            else
            {
                var membersToAdd = _applicationUserRepository.Get(u => membersInModelIds.Contains(u.Id));
                committeeModel.Members = membersToAdd.ToList();
            }
        }

        private void ChangeKudosCommittee(CommitteePostDTO modelDTO)
        {
            if (!modelDTO.IsKudosCommittee)
            {
                return;
            }

            var oldCommittee = _committeeRepository.Get(k => k.IsKudosCommittee).FirstOrDefault();

            if (oldCommittee == null)
            {
                return;
            }

            if (oldCommittee.Id == modelDTO.Id)
            {
                return;
            }

            oldCommittee.IsKudosCommittee = false;
            _committeeRepository.Update(oldCommittee);
            _unitOfWork.Save();
        }

        #endregion

        public void PutCommittee(CommitteePostDTO modelDTO)
        {
            var committeeModel = _committeeRepository
                .Get(c => c.Id == modelDTO.Id, includeProperties: "Members,Leads,Delegates")
                .FirstOrDefault();

            if (committeeModel == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.ModelNotFoundException);
            }

            ChangeKudosCommittee(modelDTO);
            CreateCommitteeModel(modelDTO, committeeModel);
            _committeeRepository.Update(committeeModel);
            _unitOfWork.Save();
        }

        public void PostCommittee(CommitteePostDTO modelDTO)
        {
            if (_committeeRepository.GetByID(modelDTO.Id) != null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.CommitteeIdException);
            }

            var committeeModel = CreateCommitteeModel(modelDTO);
            ChangeKudosCommittee(modelDTO);
            _committeeRepository.Insert(committeeModel);
            _unitOfWork.Save();
        }

        public void PostSuggestion(CommitteeSuggestionPostDTO modelDTO, string userId)
        {
            var committee = _committeeRepository.Get(c => c.Id == modelDTO.CommitteeId, includeProperties: "Suggestions, Members").FirstOrDefault();

            if (committee == null)
            {
                throw new ServiceException(Resources.Models.Committee.Committee.SuggestionCommiteNotFound);
            }

            var suggestion = _mapper.Map<CommitteeSuggestion>(modelDTO);

            suggestion.User = _applicationUserRepository.GetByID(userId);
            suggestion.Date = DateTime.UtcNow;

            committee.Suggestions.Add(suggestion);

            _committeeRepository.Update(committee);
            _unitOfWork.Save();

            var suggestionDto= new CommitteeSuggestionCreatedDto
            {
                CommitteeId = committee.Id,
                SuggestionId = suggestion.Id
            };
            _asyncRunner.Run<ICommitteeNotificationService>(n=>n.NotifyCommitteeMembersAboutNewSuggestion(suggestionDto), _uow.ConnectionName);
        }

        public IEnumerable<CommitteeSuggestionDto> GetCommitteeSuggestions(int id)
        {
            var committee = _committeeRepository.Get(c => c.Id == id, includeProperties: "Suggestions.User").FirstOrDefault();

            var committeeSuggestions = committee.Suggestions.OrderByDescending(d => d.Date);

            return _mapper.Map<IEnumerable<CommitteeSuggestionDto>>(committeeSuggestions);
        }

        public void DeleteComitteeSuggestion(int comitteeId, int suggestionId, UserAndOrganizationDTO userAndOrg)
        {
            var committee = _committteeDbSet
                .Include(u => u.Suggestions)
                .FirstOrDefault(u => u.Id == comitteeId);

            if (committee == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Committee does not exist");
            }

            var suggestion = committee.Suggestions.FirstOrDefault(x => x.Id == suggestionId);

            committee.Suggestions.Remove(suggestion);

            _uow.SaveChanges(userAndOrg.UserId);
        }

        public CommitteeViewDTO GetKudosCommittee()
        {
            var kudosCommittee = _committeeRepository.Get(k => k.IsKudosCommittee, includeProperties: "Members").FirstOrDefault();

            var kudosCommitteeMapped = _mapper.Map<CommitteeViewDTO>(kudosCommittee);

            return kudosCommitteeMapped;
        }

        public int GetKudosCommitteeId()
        {
            var kudosCommittee = _committeeRepository.Get(k => k.IsKudosCommittee).FirstOrDefault();

            if (kudosCommittee == null)
            {
                return 0;
            }

            return kudosCommittee.Id;
        }
    }
}
