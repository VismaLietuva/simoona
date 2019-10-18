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
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Lotteries;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Lottery> _lotteriesDbSet;
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;
        private readonly IMapper _mapper;
        private readonly IParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly IKudosService _kudosService;

        public LotteryService(IUnitOfWork2 uow, IMapper mapper, IParticipantService participantService, IUserService userService, IKudosService kudosService)
        {
            _uow = uow;
            _mapper = mapper;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _participantsDbSet = uow.GetDbSet<LotteryParticipant>();
            _participantService = participantService;
            _userService = userService;
            _kudosService = kudosService;
        }

        public async Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO)
        {
            if (newLotteryDTO.EndDate < DateTime.UtcNow)
            {
                throw new LotteryException("Lottery cant start in the past");
            }

            if (newLotteryDTO.EntryFee < 1)
            {
                throw new LotteryException("Invalid entry fee");
            }

            if (newLotteryDTO.Status == (int)LotteryStatus.Aborted || newLotteryDTO.Status == (int)LotteryStatus.Ended)
            {
                throw new LotteryException("Invalid status of created lottery");
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
                throw new LotteryException("Lottery has started or ended");
            }
            UpdateDraftedLottery(lottery, lotteryDTO);
            _uow.SaveChanges(false);
        }

        public void EditStartedLottery(EditStartedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == lotteryDTO.Id);

            if (lottery.Status != (int)LotteryStatus.Started)
            {
                throw new LotteryException("Lottery is not running");
            }
            lottery.Description = lotteryDTO.Description;
            _uow.SaveChanges();

        }

        public LotteryDetailsDTO GetLotteryDetails(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);
            if (lottery == null)
            {
                throw new LotteryException("Lottery not found");
            }

            var lotteryDetailsDTO = _mapper.Map<Lottery, LotteryDetailsDTO>(lottery);
            lotteryDetailsDTO.Participants = _participantsDbSet.Count(p => p.LotteryId == id);

            return lotteryDetailsDTO;
        }

        public void RemoveLottery(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.SingleOrDefault(p => p.Id == id && p.OrganizationId == userOrg.OrganizationId);
            if (lottery == null)
            {
                throw new LotteryException("Lottery not found");
            }

            lottery.Status = (int)LotteryStatus.Aborted;
            _uow.SaveChanges();
        }

        public async Task FinishLotteryAsync(int lotteryId)
        {
            var lottery = _lotteriesDbSet.Find(lotteryId);

            if (lottery == null)
            {
                throw new LotteryException("Lottery not found");
            }

            lottery.Status = (int)LotteryStatus.Ended;

            await _uow.SaveChangesAsync();
        }

        public LotteryStatsDTO GetLotteryStats(int lotteryId)
        {
            var lottery = _lotteriesDbSet.Find(lotteryId);

            if (lottery == null)
            {
                throw new LotteryException("Lottery not found.");
            }
            var participants = _participantService.GetParticipantsCounted(lotteryId);

            var ticketsSold = participants.Sum(participant => participant.Tickets);

            LotteryStatsDTO lotteryStats = new LotteryStatsDTO()
            {
                TotalParticipants = participants.Count(),
                TicketsSold = ticketsSold,
                KudosSpent = ticketsSold * lottery.EntryFee,
                Participants = participants
            };

            return lotteryStats;

        }


        public IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization)
        {
            return _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate).ToList();
        }

        public IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter)
        {
            return _lotteriesDbSet
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate);
        }

        public IPagedList<LotteryDetailsDTO> GetPagedLotteries(string filter, int page, int pageSize)
        {
            var filteredLotteries = GetFilteredLotteries(filter);
            return filteredLotteries.ToPagedList(page, pageSize);
        }

        private Lottery MapNewLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<CreateLotteryDTO, Lottery>(newLotteryDTO);

            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;

            return newLottery;
        }

        public async Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            ApplicationUser applicationUser = _userService.GetApplicationUser(userOrg.UserId);

            LotteryDetailsDTO lotteryDetails = GetLotteryDetails(lotteryTicketDTO.LotteryId, userOrg);

            if (applicationUser.RemainingKudos < lotteryDetails.EntryFee * lotteryTicketDTO.Tickets)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }

            for (int i = 0; i < lotteryTicketDTO.Tickets; i++)
            {
                LotteryParticipant participant = MapNewLotteryParticipant(lotteryTicketDTO, userOrg);

                _participantsDbSet.Add(participant);

            }

            AddKudosLogDTO kudosLogDTO = new AddKudosLogDTO()
            {
                ReceivingUserIds = new List<string>() { userOrg.UserId },
                PointsTypeId = 2,
                MultiplyBy = lotteryTicketDTO.Tickets * lotteryDetails.EntryFee,
                Comment = $"For {lotteryTicketDTO.Tickets} tickets",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLog(kudosLogDTO, userOrg);

            await _uow.SaveChangesAsync(applicationUser.Id);
        }

        public IEnumerable<LotteryDetailsDTO> GetRunningLotteries(UserAndOrganizationDTO userAndOrganization)
        {
            var lotteries = _lotteriesDbSet.
                Where(p => p.OrganizationId == userAndOrganization.OrganizationId
                && p.Status == (int)LotteryStatus.Started
                && p.EndDate > DateTime.UtcNow)
                .Select(MapLotteriesToListItemDto)
                .OrderBy(p => p.EndDate).ToList();

            return lotteries;
        }

        private readonly Expression<Func<LotteryDetailsDTO, DateTime>> ByEndDate = e => e.EndDate;

        private Expression<Func<Lottery, LotteryDetailsDTO>> MapLotteriesToListItemDto =>
         e => new LotteryDetailsDTO
         {
             Id = e.Id,
             Title = e.Title,
             Description = e.Description,
             EntryFee = e.EntryFee,
             EndDate = e.EndDate,
             Status = e.Status
         };


        private void UpdateDraftedLottery(Lottery lottery, EditDraftedLotteryDTO draftedLotteryDTO)
        {
            lottery.EntryFee = draftedLotteryDTO.EntryFee;
            lottery.EndDate = draftedLotteryDTO.EndDate;
            lottery.Description = draftedLotteryDTO.Description;
            lottery.Status = draftedLotteryDTO.Status;
            lottery.Title = draftedLotteryDTO.Title;
            lottery.Images = draftedLotteryDTO.Images;
        }

        private LotteryParticipant MapNewLotteryParticipant(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            LotteryParticipant participant = new LotteryParticipant()
            {
                LotteryId = lotteryTicketDTO.LotteryId,
                UserId = userOrg.UserId,
                Entered = DateTime.Now,
                CreatedBy = userOrg.UserId,
                ModifiedBy = userOrg.UserId,
                Modified = DateTime.Now,
                Created = DateTime.Now
            };

            return participant;
        }
    }
}
