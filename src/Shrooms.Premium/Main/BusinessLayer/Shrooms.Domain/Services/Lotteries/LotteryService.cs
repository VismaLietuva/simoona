using AutoMapper;
using PagedList;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Args;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.FireAndForget;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Lottery> _lotteriesDbSet;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IDbSet<LotteryParticipant> _participantsDbSet;
        private readonly IMapper _mapper;
        private readonly IParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly IKudosService _kudosService;

        public LotteryService(IUnitOfWork2 uow, IMapper mapper, IParticipantService participantService,
            IUserService userService, IKudosService kudosService, IAsyncRunner asyncRunner)
        {
            _uow = uow;
            _lotteriesDbSet = uow.GetDbSet<Lottery>();
            _asyncRunner = asyncRunner;
            _mapper = mapper;
            _participantsDbSet = uow.GetDbSet<LotteryParticipant>();
            _participantService = participantService;
            _userService = userService;
            _kudosService = kudosService;
        }

        public async Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO)
        {
            if (newLotteryDTO.EndDate < DateTime.UtcNow)
            {
                throw new LotteryException("Lottery can't start in the past.");
            }

            if (newLotteryDTO.EntryFee < 1)
            {
                throw new LotteryException("Invalid entry fee");
            }

            if (newLotteryDTO.Status != (int)LotteryStatus.Started ||
                newLotteryDTO.Status != (int)LotteryStatus.Drafted)
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
            var lottery = _lotteriesDbSet.Find(lotteryDTO.Id);

            if (lottery.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Lottery has started or ended");
            }

            UpdateDraftedLottery(lottery, lotteryDTO);

            _uow.SaveChanges(false);
        }

        public void EditStartedLottery(EditStartedLotteryDTO lotteryDTO)
        {
            var lottery = _lotteriesDbSet.Find(lotteryDTO.Id);

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

        public void AbortLottery(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);

            if (lottery != null)
            {
                if (lottery.Status == (int)LotteryStatus.Started)
                {
                    _asyncRunner.Run<ILotteryAbortJob>(n => n.RefundLottery(lottery, userOrg), _uow.ConnectionName);
                    lottery.Status = (int)LotteryStatus.RefundStarted;
                }
                else
                {
                    lottery.Status = (int)LotteryStatus.Aborted;
                }

                _uow.SaveChanges();
            }
        }

        public void RefundParticipants(int id, UserAndOrganizationDTO userOrg)
        {
            var lottery = _lotteriesDbSet.Find(id);

            if (lottery != null)
            {
                if (lottery.Status == (int)LotteryStatus.RefundFailed)
                {
                    _asyncRunner.Run<ILotteryAbortJob>(n => n.RefundLottery(lottery, userOrg), _uow.ConnectionName);
                }
            }
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

            return new LotteryStatsDTO
            {
                TotalParticipants = participants.Count(),
                TicketsSold = ticketsSold,
                KudosSpent = ticketsSold * lottery.EntryFee,
                Participants = participants
            };
        }

        public IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization)
        {
            return _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate);
        }

        public IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter, UserAndOrganizationDTO userOrg)
        {
            return _lotteriesDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(ByEndDate);
        }

        public IPagedList<LotteryDetailsDTO> GetPagedLotteries(GetPagedLotteriesArgs args)
        {
            var filteredLotteries = GetFilteredLotteries(args.Filter, args.UserOrg);
            return filteredLotteries.ToPagedList(args.PageNumber, args.PageSize);
        }

        public int GetLotteryStatus(int id)
        {
            return _lotteriesDbSet.Find(id).Status;
        }

        public void EditLotteryStatus(int id, LotteryStatus status)
        {
            var lottery = _lotteriesDbSet.Find(id);

            lottery.Status = (int)status;

            _uow.SaveChanges();
        }

        public async Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            var applicationUser = _userService.GetApplicationUser(userOrg.UserId);

            var lotteryDetails = GetLotteryDetails(lotteryTicketDTO.LotteryId, userOrg);

            if (applicationUser.RemainingKudos < lotteryDetails.EntryFee * lotteryTicketDTO.Tickets)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }

            for (int i = 0; i < lotteryTicketDTO.Tickets; i++)
            {
                _participantsDbSet.Add(MapNewLotteryParticipant(lotteryTicketDTO, userOrg));
            }

            var kudosLogDTO = new AddKudosLogDTO()
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
            return _lotteriesDbSet.
                Where(p =>
                    p.OrganizationId == userAndOrganization.OrganizationId &&
                    p.Status == (int)LotteryStatus.Started &&
                    p.EndDate > DateTime.UtcNow)
                .Select(MapLotteriesToListItemDto)
                .OrderBy(ByEndDate);
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

        private Lottery MapNewLottery(CreateLotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<CreateLotteryDTO, Lottery>(newLotteryDTO);

            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;

            return newLottery;
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

        private LotteryParticipant MapNewLotteryParticipant(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            return new LotteryParticipant
            {
                LotteryId = lotteryTicketDTO.LotteryId,
                UserId = userOrg.UserId,
                Joined = DateTime.Now,
                CreatedBy = userOrg.UserId,
                ModifiedBy = userOrg.UserId,
                Modified = DateTime.Now,
                Created = DateTime.Now
            };
        }
    }
}