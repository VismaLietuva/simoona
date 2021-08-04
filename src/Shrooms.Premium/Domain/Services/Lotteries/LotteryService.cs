using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.UserService;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public class LotteryService : ILotteryService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<Lottery> _lotteriesDbSet;
        private readonly DbSet<LotteryParticipant> _participantsDbSet;
        private readonly IAsyncRunner _asyncRunner;
        private readonly IMapper _mapper;
        private readonly IParticipantService _participantService;
        private readonly IUserService _userService;
        private readonly IKudosService _kudosService;

        public LotteryService(IUnitOfWork2 uow,
            IMapper mapper,
            IParticipantService participantService,
            IUserService userService,
            IKudosService kudosService,
            IAsyncRunner asyncRunner)
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

        public async Task<Lottery> GetLotteryAsync(int lotteryId)
        {
            return await _lotteriesDbSet.FindAsync(lotteryId);
        }

        public async Task<LotteryDTO> CreateLotteryAsync(LotteryDTO newLotteryDTO)
        {
            if (newLotteryDTO.EndDate < DateTime.UtcNow)
            {
                throw new LotteryException("Lottery can't start in the past.");
            }

            if (newLotteryDTO.EntryFee < 1)
            {
                throw new LotteryException("Invalid entry fee.");
            }

            if (newLotteryDTO.Status != (int)LotteryStatus.Started &&
                newLotteryDTO.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Invalid status of created lottery.");
            }

            var newLottery = MapNewLottery(newLotteryDTO);
            _lotteriesDbSet.Add(newLottery);

            await _uow.SaveChangesAsync(newLotteryDTO.UserId);

            newLotteryDTO.Id = newLottery.Id;

            return newLotteryDTO;
        }

        public async Task EditDraftedLotteryAsync(LotteryDTO lotteryDTO)
        {
            var lottery = await _lotteriesDbSet.FindAsync(lotteryDTO.Id);

            if (lottery != null && lottery.Status != (int)LotteryStatus.Drafted)
            {
                throw new LotteryException("Editing is forbidden for not drafted lottery.");
            }

            UpdateDraftedLottery(lottery, lotteryDTO);

            await _uow.SaveChangesAsync(false);
        }

        public async Task EditStartedLotteryAsync(EditStartedLotteryDTO lotteryDTO)
        {
            var lottery = await _lotteriesDbSet.FindAsync(lotteryDTO.Id);

            if (lottery != null && lottery.Status != (int)LotteryStatus.Started)
            {
                throw new LotteryException("Lottery is not running.");
            }

            if (lottery != null)
            {
                lottery.Description = lotteryDTO.Description;
                await _uow.SaveChangesAsync(false);
            }
        }

        public async Task<LotteryDetailsDTO> GetLotteryDetailsAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            var lotteryDetailsDTO = _mapper.Map<Lottery, LotteryDetailsDTO>(lottery);
            lotteryDetailsDTO.Participants = await _participantsDbSet.CountAsync(p => p.LotteryId == lotteryId);

            return lotteryDetailsDTO;
        }

        public async Task<bool> AbortLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return false;
            }

            if (lottery.Status == (int)LotteryStatus.Started)
            {
                lottery.Status = (int)LotteryStatus.RefundStarted;
                await _uow.SaveChangesAsync();

                _asyncRunner.Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lottery.Id, userOrg), _uow.ConnectionName);
            }
            else if (lottery.Status == (int)LotteryStatus.Drafted)
            {
                lottery.Status = (int)LotteryStatus.Deleted;
                await _uow.SaveChangesAsync();
            }

            return lottery.Status == (int)LotteryStatus.Deleted || lottery.Status == (int)LotteryStatus.RefundStarted;
        }

        public async Task RefundParticipantsAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            if (!lottery.IsRefundFailed)
            {
                return;
            }

            lottery.IsRefundFailed = false;
            await _uow.SaveChangesAsync(userOrg.UserId);

            _asyncRunner.Run<ILotteryAbortJob>(async notifier => await notifier.RefundLotteryAsync(lottery.Id, userOrg), _uow.ConnectionName);
        }

        public async Task UpdateRefundFailedFlagAsync(int lotteryId, bool isFailed, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet.FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            lottery.IsRefundFailed = isFailed;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet.FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return;
            }

            lottery.Status = (int)LotteryStatus.Ended;

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task<LotteryStatsDTO> GetLotteryStatsAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            var participants = await _participantService.GetParticipantsCountedAsync(lotteryId);

            var ticketsSold = participants.Sum(participant => participant.Tickets);

            return new LotteryStatsDTO
            {
                TotalParticipants = participants.Count,
                TicketsSold = ticketsSold,
                KudosSpent = ticketsSold * lottery.EntryFee
            };
        }

        public async Task<IEnumerable<LotteryDetailsDTO>> GetLotteriesAsync(UserAndOrganizationDTO userOrganization)
        {
            return await _lotteriesDbSet
                .Where(p => p.OrganizationId == userOrganization.OrganizationId)
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(_byEndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LotteryDetailsDTO>> GetFilteredLotteriesAsync(string filter, UserAndOrganizationDTO userOrg)
        {
            return await _lotteriesDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId)
                .Where(x => x.Title.Contains(filter))
                .Select(MapLotteriesToListItemDto)
                .OrderByDescending(x => x.RefundFailed)
                .ThenByDescending(x => x.Status == (int)LotteryStatus.Started || x.Status == (int)LotteryStatus.Drafted)
                .ThenByDescending(_byEndDate)
                .ToListAsync();
        }

        public async Task<IPagedList<LotteryDetailsDTO>> GetPagedLotteriesAsync(GetPagedLotteriesArgs args)
        {
            var filteredLotteries = await GetFilteredLotteriesAsync(args.Filter, args.UserOrg);
            return await filteredLotteries.ToPagedListAsync(args.PageNumber, args.PageSize);
        }

        public async Task<LotteryStatusDTO> GetLotteryStatusAsync(int lotteryId, UserAndOrganizationDTO userOrg)
        {
            var lottery = await _lotteriesDbSet
                .FirstOrDefaultAsync(x => x.Id == lotteryId && x.OrganizationId == userOrg.OrganizationId);

            if (lottery is null)
            {
                return null;
            }

            return new LotteryStatusDTO
            {
                LotteryStatus = lottery.Status,
                RefundFailed = lottery.IsRefundFailed
            };
        }

        public async Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
        {
            var applicationUser = await _userService.GetApplicationUserAsync(userOrg.UserId);

            var lotteryDetails = await GetLotteryDetailsAsync(lotteryTicketDTO.LotteryId, userOrg);

            if (lotteryDetails is null || applicationUser is null)
            {
                return;
            }

            if (lotteryTicketDTO.Tickets <= 0)
            {
                await AddKudosLogForCheatingAsync(userOrg, lotteryDetails.EntryFee, applicationUser);
                throw new LotteryException("Thanks for trying - you were charged double Kudos for this without getting a ticket.");
            }

            if (applicationUser.RemainingKudos < lotteryDetails.EntryFee * lotteryTicketDTO.Tickets)
            {
                throw new LotteryException("User does not have enough kudos for the purchase.");
            }

            if (DateTime.UtcNow > lotteryDetails.EndDate)
            {
                throw new LotteryException("Lottery has already ended.");
            }

            var kudosLogDTO = new AddKudosLogDTO
            {
                ReceivingUserIds = new List<string> { userOrg.UserId },
                PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus),
                MultiplyBy = lotteryTicketDTO.Tickets * lotteryDetails.EntryFee,
                Comment = $"{lotteryTicketDTO.Tickets} ticket(s) for lottery {lotteryDetails.Title}",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLogDTO, userOrg);

            if (applicationUser.RemainingKudos < 0)
            {
                kudosLogDTO.PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Refund);
                await _kudosService.AddRefundKudosLogsAsync(new List<AddKudosLogDTO> { kudosLogDTO });
            }
            else
            {
                for (var i = 0; i < lotteryTicketDTO.Tickets; i++)
                {
                    _participantsDbSet.Add(MapNewLotteryParticipant(lotteryTicketDTO, userOrg));
                }
            }

            await _uow.SaveChangesAsync(applicationUser.Id);
            await _kudosService.UpdateProfileKudosAsync(applicationUser, userOrg);
        }

        private async Task AddKudosLogForCheatingAsync(UserAndOrganizationDTO userOrg, int entryFee, ApplicationUser applicationUser)
        {
            var kudosLogDTO = new AddKudosLogDTO
            {
                ReceivingUserIds = new List<string> { userOrg.UserId },
                PointsTypeId = await _kudosService.GetKudosTypeIdAsync(KudosTypeEnum.Minus),
                MultiplyBy = entryFee * 2,
                Comment = "Cheating",
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId
            };

            await _kudosService.AddLotteryKudosLogAsync(kudosLogDTO, userOrg);
            await _uow.SaveChangesAsync(applicationUser.Id);
            await _kudosService.UpdateProfileKudosAsync(applicationUser, userOrg);
        }

        public async Task<List<LotteryDetailsDTO>> GetRunningLotteriesAsync(UserAndOrganizationDTO userAndOrganization)
        {
            return await _lotteriesDbSet.Where(p =>
                    p.OrganizationId == userAndOrganization.OrganizationId &&
                    p.Status == (int)LotteryStatus.Started &&
                    p.EndDate > DateTime.UtcNow)
                .Select(MapLotteriesToListItemDto)
                .OrderBy(_byEndDate)
                .ToListAsync();
        }

        private readonly Expression<Func<LotteryDetailsDTO, DateTime>> _byEndDate = e => e.EndDate;

        private static Expression<Func<Lottery, LotteryDetailsDTO>> MapLotteriesToListItemDto =>
            e => new LotteryDetailsDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EntryFee = e.EntryFee,
                EndDate = e.EndDate,
                Status = e.Status,
                RefundFailed = e.IsRefundFailed
            };

        private Lottery MapNewLottery(LotteryDTO newLotteryDTO)
        {
            var newLottery = _mapper.Map<LotteryDTO, Lottery>(newLotteryDTO);

            newLottery.CreatedBy = newLotteryDTO.UserId;
            newLottery.Modified = DateTime.UtcNow;
            newLottery.ModifiedBy = newLotteryDTO.UserId;
            newLottery.IsRefundFailed = false;

            return newLottery;
        }

        private static void UpdateDraftedLottery(Lottery lottery, LotteryDTO draftedLotteryDTO)
        {
            lottery.EntryFee = draftedLotteryDTO.EntryFee;
            lottery.EndDate = draftedLotteryDTO.EndDate;
            lottery.Description = draftedLotteryDTO.Description;
            lottery.Status = draftedLotteryDTO.Status;
            lottery.Title = draftedLotteryDTO.Title;
            lottery.Images = draftedLotteryDTO.Images;
        }

        private static LotteryParticipant MapNewLotteryParticipant(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg)
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
