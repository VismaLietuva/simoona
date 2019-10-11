using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using PagedList;
using Shrooms.Constants.WebApi;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.EntityModels.Models.Lotteries;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Lottery> _lotteriesDbSet;
        private readonly IDbSet<LotteryParticipant> _lotteryParticipantsDbSet;
        private readonly IMapper _mapper;

        public LotteryService(IUnitOfWork2 uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _lotteryParticipantsDbSet = uow.GetDbSet<LotteryParticipant>();
        }

        public async Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO)
        {
            if (newLotteryDTO.EndDate < DateTime.UtcNow)
            {
                // exception (cant create lottery in past)
            }
            var newLottery = MapNewLottery(newLotteryDTO);
            _lotteriesDbSet.Add(newLottery);
            await _uow.SaveChangesAsync(newLotteryDTO.UserId);

            newLotteryDTO.Id = newLottery.Id;

            return newLotteryDTO;
        }

        public void EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == lotteryDTO.Id);
            if (lottery.Status != (int)LotteryStatus.Drafted)
            {
                // exception (can only edit drafted lottery)
            }
            UpdateDraftedLottery(lottery, lotteryDTO);
            _uow.SaveChanges(false);
        }

        public void EditStartedLottery(EditStartedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == lotteryDTO.Id);

            if (lottery.Status != (int)LotteryStatus.Started)
            {
                // exception (Lottery has started or ended)
            }
            lottery.Description = lotteryDTO.Description;
            _uow.SaveChanges();

        }

        public LotteryDetailsDTO GetLotteryDetails(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);
            if (lottery == null)
            {
                // exception (Lottery not found)
            }

            var lotteryDetailsDTO = _mapper.Map<Lottery, LotteryDetailsDTO>(lottery);
            lotteryDetailsDTO.Participants = _lotteryParticipantsDbSet.Count(p => p.LotteryId == id);

            return lotteryDetailsDTO;
        }

        public void RemoveLottery(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == id && p.OrganizationId == userOrg.OrganizationId);
            lottery.Status = (int)LotteryStatus.Aborted;


            //   _lotteriesDbSet.Remove(lottery);

            _uow.SaveChanges();


        }

        public IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization)
        {
            var lotteries = _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto())
                .OrderBy(p => p.EndDate).ToList();

            return lotteries;
        }

        public IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter)
        {
            var lotteries = _lotteriesDbSet
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto())
                .OrderByDescending(x => x.EndDate)
                .ToList();

            return lotteries;
        }

        public IPagedList<LotteryDetailsDTO> GetPagedLotteries(string filter, int page, int pageSize)
        {
            var filteredLotteries = GetFilteredLotteries(filter);
            return filteredLotteries.ToPagedList(page, pageSize);
        }

        private Lottery MapNewLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<CreateLotteryDTO, Lottery>(newLotteryDTO);

            newLottery.Created = newLotteryDTO.StartDate;
            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;

            return newLottery;
        }

        private Expression<Func<Lottery, LotteryDetailsDTO>> MapLotteriesToListItemDto()
        {
            return e => new LotteryDetailsDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EntryFee = e.EntryFee,
                Images = e.Images,
                EndDate = e.EndDate,
                Status = e.Status,
                Participants = _lotteryParticipantsDbSet.Count(p => p.LotteryId == e.Id)
        };
        }

        private void UpdateDraftedLottery(Lottery lottery, EditDraftedLotteryDTO draftedLotteryDTO)
        {
            lottery.EntryFee = draftedLotteryDTO.EntryFee;
            lottery.EndDate = draftedLotteryDTO.EndDate;
            lottery.Description = draftedLotteryDTO.Description;
            lottery.Status = draftedLotteryDTO.Status;
            lottery.Title = draftedLotteryDTO.Title;
            lottery.Images = draftedLotteryDTO.Images;
        }

        public IEnumerable<LotteryDetailsDTO> GetLotteriesByStatus(int status, UserAndOrganizationDTO userAndOrganization)
        {
            var lotteries = _lotteriesDbSet
               .Where(p => p.OrganizationId == userAndOrganization.OrganizationId && p.Status == status)
               .Select(MapLotteriesToListItemDto())
               .OrderBy(p => p.EndDate).ToList();


            return lotteries;
        }
    }
}
